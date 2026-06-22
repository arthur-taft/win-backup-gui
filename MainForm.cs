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
                return Path.Combine(drive.RootDirectory.FullName.TrimEnd('\\'), Environment.UserName);
            }
        }
        private Dictionary<string, List<string>> _excludedFiles = new Dictionary<string, List<string>>();

        public MainForm()
        {
            InitializeComponent(); // Always first — loads the Designer layout

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
            _backupItems = BackupEngine.BuildBackupItems();

            clbLocations.Items.Clear();
            foreach (var item in _backupItems)
            {
                int index = clbLocations.Items.Add(item.Name);
                clbLocations.SetItemChecked(index, item.Enabled);
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
    }
}
