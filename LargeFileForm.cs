/*
 * Win-Backup
 * LargeFileForm.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
using win_backup.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace win_backup
{
    public class LargeFileForm : Form
    {
        private CheckedListBox _checklist;
        private Button _btnConfirm;
        private Button _btnCancel;

        private readonly List<LargeFileItem> _files;

        public LargeFileForm(string itemName, List<LargeFileItem> files)
        {
            _files = files;
            SetupForm(itemName);
        }

        private void SetupForm(string itemName)
        {
            Text = $"Large Files Found — {itemName}";
            Size = new Size(700, 440);
            MinimumSize = new Size(500, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            var lblHeader = new Label
            {
                Text = $"Files over 5 GB were found in \"{itemName}\".\n" +
                             "Uncheck any you want to skip — checked files will be copied.",
                Dock = DockStyle.Top,
                Height = 48,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9),
                Padding = new Padding(10, 0, 0, 0)
            };

            _checklist = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                Font = new Font("Consolas", 9)
            };

            foreach (var file in _files)
            {
                int index = _checklist.Items.Add(file.DisplayName);
                _checklist.SetItemChecked(index, file.Enabled);
            }

            // Keep the model in sync as the user checks/unchecks
            _checklist.ItemCheck += (s, e) =>
                _files[e.Index].Enabled = (e.NewValue == CheckState.Checked);

            // Bottom panel with Confirm and Cancel
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 55 };

            _btnConfirm = new Button
            {
                Text = "Confirm && Continue",
                Size = new Size(160, 35),
                Location = new Point(10, 10),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            _btnConfirm.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            _btnCancel = new Button
            {
                Text = "Cancel Backup",
                Size = new Size(130, 35),
                Location = new Point(180, 10),
                ForeColor = Color.DarkRed
            };
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            bottomPanel.Controls.Add(_btnConfirm);
            bottomPanel.Controls.Add(_btnCancel);

            Controls.Add(_checklist);
            Controls.Add(lblHeader);
            Controls.Add(bottomPanel);
        }
    }
}
