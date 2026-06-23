/*
 * Win-Backup
 * MainForm.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
using win_backup.Helpers;
using win_backup.Models;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace win_backup
{
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
        private const uint BCM_SETSHIELD = 0x160C;
        // These are "fields" — data that the whole form can access
        private List<BackupItem> _backupItems;
        private Dictionary<string, DriveInfo> _driveMapping;
        private string DestinationRoot
        {
            get
            {
                if (cboDrive.SelectedItem == null) return string.Empty;
                var drive = _driveMapping[cboDrive.SelectedItem.ToString()];

                // RootDirectory returns "<DriveLetter>:\" - Trim end removes backslash
                return Path.Combine(drive.RootDirectory.FullName.TrimEnd('\\'), _targetUser);
            }
        }
        private Dictionary<string, List<string>> _excludedFiles = new Dictionary<string, List<string>>();
        
        private string _targetUser = Environment.UserName;
        public MainForm(string[] args)
        {
            InitializeComponent(); // Always first — loads the Designer layout

            btnDifferentUser.FlatStyle = FlatStyle.System;
            SendMessage(btnDifferentUser.Handle, BCM_SETSHIELD, 0, 1);

            LoadDrives();          // Fill the drive dropdown
            LoadBackupLocations(); // Fill the locations checklist
        }

        private void LoadDrives()
        {
            _driveMapping = BackupEngine.GetDrives();

            cboDrive.Items.Clear();
            foreach (var displayText in _driveMapping.Keys)
                cboDrive.Items.Add(displayText);

            if (cboDrive.Items.Count > 0)
                cboDrive.SelectedIndex = 0; // Auto-select the first drive
        }

        private void LoadBackupLocations()
        {
            _backupItems = BackupEngine.BuildBackupItems(_targetUser);

            clbLocations.Items.Clear();
            foreach (var item in _backupItems)
            {
                int index = clbLocations.Items.Add(item.Name);
                clbLocations.SetItemChecked(index, item.Enabled);
            }
        }
        private void ShowUserPicker()
        {
            var users = BackupEngine.GetUserProfiles();

            if (users.Count == 0)
            {
                MessageBox.Show("No user profiles found.", "No Users",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Simple selection via a small input dialog
            using (var picker = new UserPickerForm(users, _targetUser))
            {
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    _targetUser = picker.SelectedUser;
                    Text = $"Backup Utility — Backing up: {_targetUser}";
                    LoadBackupLocations(); // rebuild the list for the new user
                }
            }
        }

        // Keeps our _backupItems list in sync when the user checks/unchecks a location
        private void clbLocations_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _backupItems[e.Index].Enabled = (e.NewValue == CheckState.Checked);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            // Pass current data into the preview form
            var preview = new PreviewForm(_backupItems, DestinationRoot);
            preview.ShowDialog();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            int selectedCount = _backupItems.Count(item => item.Enabled);

            if (selectedCount == 0)
            {
                MessageBox.Show(
                    "Please select at least one location to back up.",
                    "No Locations Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _excludedFiles.Clear();

            // --- Large File Audit ---
            if (chkLargeFileAudit.Checked)
            {
                btnStart.Enabled = false;
                btnStart.Text = "Scanning for large files...";

                foreach (var item in _backupItems.Where(i => i.Enabled))
                {
                    // Run the scan on a background thread so the UI doesn't freeze
                    var largeFiles = await Task.Run(() => BackupEngine.ScanLargeFiles(item.Src));

                    if (largeFiles.Count > 0)
                    {
                        var auditForm = new LargeFileForm(item.Name, largeFiles);

                        if (auditForm.ShowDialog() == DialogResult.Cancel)
                        {
                            // User hit Cancel — mirrors $userCancelled = $true in your PS script
                            btnStart.Enabled = true;
                            btnStart.Text = "Start Backup";
                            return;
                        }

                        // Collect unchecked files into the exclusion list for robocopy /XF
                        var excluded = largeFiles
                            .Where(f => !f.Enabled)
                            .Select(f => f.FileName)
                            .ToList();

                        if (excluded.Count > 0)
                            _excludedFiles[item.Name] = excluded;
                    }
                }

                btnStart.Enabled = true;
                btnStart.Text = "Start Backup";
            }

            // --- Launch the progress form ---
            var progressForm = new ProgressForm(_backupItems, DestinationRoot, _excludedFiles, chkSizeCheck.Checked);
            progressForm.ShowDialog();
        }

        private void btnAddPath_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder to include in the backup";
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() != DialogResult.OK) return;

                string selectedPath = dialog.SelectedPath;
                string folderName = Path.GetFileName(selectedPath);

                // Don't allow duplicates
                if (_backupItems.Any(i => i.Src.Equals(selectedPath, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show(
                        "That folder is already in the list.",
                        "Duplicate Path",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Build a display name that makes custom paths easy to identify
                string displayName = $"[Custom] {folderName}";

                var newItem = new BackupItem
                {
                    Name = displayName,
                    Src = selectedPath,
                    Dest = folderName, // backs up to destRoot\FolderName
                    Enabled = true
                };

                _backupItems.Add(newItem);
                int index = clbLocations.Items.Add(displayName);
                clbLocations.SetItemChecked(index, true);
            }
        }

        private void btnRemovePath_Click(object sender, EventArgs e)
        {
            int selected = clbLocations.SelectedIndex;

            if (selected == -1)
            {
                MessageBox.Show(
                    "Please select a location from the list first.",
                    "Nothing Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Only allow removing custom paths — protect the auto-detected ones
            if (!_backupItems[selected].Name.StartsWith("[Custom]"))
            {
                MessageBox.Show(
                    "Only custom paths can be removed.\n" +
                    "Use the checkbox to disable built-in locations instead.",
                    "Cannot Remove",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _backupItems.RemoveAt(selected);
            clbLocations.Items.RemoveAt(selected);
        }

        private void btnDifferentUser_Click(object sender, EventArgs e)
        {
            if (BackupEngine.IsElevated())
            {
                ShowUserPicker();
                return;
            }

            if (BackupEngine.RelaunchElevated("--pickuser"))
            {
                Application.Exit();
            }
        }
    }
}
