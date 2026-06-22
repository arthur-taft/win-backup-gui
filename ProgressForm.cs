using win_backup.Helpers;
using win_backup.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace win_backup
{
    public class ProgressForm : Form
    {
        // Controls
        private Label _lblStatus;
        private ProgressBar _progressBar;
        private Label _lblCurrentFile;
        private ListView _lstItems;
        private Button _btnClose;
        private LogForm _logForm;
        private Button _btnToggleLog;

        // Data passed in from MainForm
        private readonly List<BackupItem> _items;
        private readonly string _destRoot;
        private readonly Dictionary<string, List<string>> _excludedFiles;
        private readonly bool _sizeCheckEnabled;

        public ProgressForm(List<BackupItem> items, string destRoot, Dictionary<string, List<string>> excludedFiles, bool sizeCheckEnabled)
        {
            _items = items;
            _destRoot = destRoot;
            _excludedFiles = excludedFiles;
            _sizeCheckEnabled = sizeCheckEnabled;

            SetupForm();
            Load += ProgressForm_Load;
        }

        private void SetupForm()
        {
            Text = "Backup In Progress";
            Size = new Size(660, 500);
            MinimumSize = new Size(500, 380);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            // Status label — shows which item is currently running
            _lblStatus = new Label
            {
                Text = "Preparing...",
                Dock = DockStyle.Top,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(8, 0, 0, 0)
            };

            // Overall progress bar
            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 22,
                Style = ProgressBarStyle.Continuous
            };

            // Current file — shows the live robocopy log line
            _lblCurrentFile = new Label
            {
                Text = "",
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 8),
                ForeColor = Color.Gray,
                Padding = new Padding(8, 0, 0, 0)
            };

            // Item list — one row per backup location
            _lstItems = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9)
            };
            _lstItems.Columns.Add("Location", 450);
            _lstItems.Columns.Add("Status", 130);

            // Pre-fill the list so the user sees all locations upfront
            foreach (var item in _items)
            {
                var row = new ListViewItem(item.Name);
                row.SubItems.Add(item.Enabled ? "Waiting" : "Skipped");
                row.ForeColor = item.Enabled ? Color.Black : Color.Gray;
                _lstItems.Items.Add(row);
            }

            // Bottom panel + Close button (disabled until backup finishes)
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            _btnClose = new Button 
            {
                Text = "Close",
                Size = new Size(100, 35),
                Enabled = false
            };
            _btnClose.Click += (s, e) => Close();
            _btnToggleLog = new Button
            {
                Text = "View Log",
                Size = new Size(90, 35)
            };
            _btnToggleLog.Click += (s, e) =>
            {
                // If already open, just bring to front
                if (_logForm != null && !_logForm.IsDisposed)
                {
                    _logForm.Focus();
                    return;
                }
                _logForm = new LogForm();
                _logForm.Show(this);
            };
            bottomPanel.Controls.Add(_btnToggleLog);
            bottomPanel.Controls.Add(_btnClose);
            bottomPanel.Resize += (s, e) =>
            {
                _btnClose.Left = (bottomPanel.Width - _btnClose.Width) / 2;
                _btnClose.Top = (bottomPanel.Height - _btnClose.Height) / 2;
                _btnToggleLog.Left = bottomPanel.Width - _btnToggleLog.Width - 10;
                _btnToggleLog.Top = (bottomPanel.Height - _btnToggleLog.Height) / 2;
            };

            // Dock order matters — Fill must be added before Top/Bottom
            Controls.Add(_lstItems);
            Controls.Add(_lblCurrentFile);
            Controls.Add(_progressBar);
            Controls.Add(_lblStatus);
            Controls.Add(bottomPanel);
        }

        // Fires automatically when the form opens
        private async void ProgressForm_Load(object sender, EventArgs e)
        {
            var progress = new Progress<CopyProgress>(UpdateUI);

            // RunBackup now returns the logs dictionary
            var auditLogs = await Task.Run(() =>
                BackupEngine.RunBackup(_items, _destRoot, _excludedFiles, progress));

            _lblStatus.Text = "Backup complete!";
            _lblStatus.ForeColor = Color.DarkGreen;
            _lblCurrentFile.Text = "";

            // --- Post-Copy Audit (mirrors your Phase 5) ---
            if (_sizeCheckEnabled && auditLogs.Count > 0)
            {
                _lblStatus.Text = "Auditing transfer logs...";
                var failures = await Task.Run(() => BackupEngine.VerifyLogs(auditLogs));
                _lblStatus.Text = "Backup complete!";
                _lblStatus.ForeColor = Color.DarkGreen;

                // Open as modal so the user reviews results before closing
                new AuditForm(failures).ShowDialog(this);
            }

            _btnClose.Enabled = true;
        }

        // Called automatically by Progress<T> — always runs on UI thread
        private void UpdateUI(CopyProgress p)
        {
            _progressBar.Maximum = p.TotalItems;
            _progressBar.Value = Math.Min(p.ItemIndex, p.TotalItems);

            if (p.Status == "Copying")
                _lblStatus.Text = $"Copying: {p.ItemName}";
            else if (p.Status == "Complete")
                _lblStatus.Text = $"Completed: {p.ItemName}";

            if (!string.IsNullOrEmpty(p.CurrentFile))
                _lblCurrentFile.Text = p.CurrentFile;

            // Find and update the matching row in the list
            foreach (ListViewItem row in _lstItems.Items)
            {
                if (row.Text != p.ItemName) continue;

                row.SubItems[1].Text = p.Status;
                row.ForeColor = p.Status switch
                {
                    "Complete" => Color.DarkGreen,
                    "Copying" => Color.DarkBlue,
                    "Skipped" => Color.Gray,
                    "Not Found" => Color.DarkOrange,
                    _ => Color.Black
                };
                row.EnsureVisible(); // Auto-scroll to keep active item visible
                break;
            }
        }
    }
}