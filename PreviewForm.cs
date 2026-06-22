using win_backup.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace win_backup
{
    public class PreviewForm : Form
    {
        private ListView _listView;
        private Button _btnClose;

        // The form receives the data it needs through its constructor
        public PreviewForm(List<BackupItem> items, string destRoot)
        {
            SetupForm();
            PopulateList(items, destRoot);
        }

        private void SetupForm()
        {
            // Form properties
            Text = "Destination Path Preview";
            Size = new Size(800, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;

            // ListView — the main content area
            _listView = new ListView
            {
                Dock = DockStyle.Fill,         // Fills the top of the form
                View = View.Details,           // Table-style with columns
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 9)
            };

            // Add columns
            _listView.Columns.Add("Status", 80);
            _listView.Columns.Add("Name", 140);
            _listView.Columns.Add("From", 230);
            _listView.Columns.Add("To", 230);

            // Allow columns to be resized by double-clicking borders
            _listView.ColumnClick += (s, e) => _listView.Columns[e.Column].Width = -2;

            // A panel pinned to the bottom of the form — holds the Close button
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            _btnClose = new Button
            {
                Text = "Close",
                Size = new Size(100, 35),
            };
            _btnClose.Click += (s, e) => Close();

            bottomPanel.Resize += (s, e) =>
            {
                _btnClose.Left = (bottomPanel.Width - _btnClose.Width) / 2;
                _btnClose.Top = (bottomPanel.Height - _btnClose.Height) / 2;
            };

            bottomPanel.Controls.Add(_btnClose);

            // Order matters here — add Fill controls before Bottom controls
            Controls.Add(_listView);
            Controls.Add(bottomPanel);
        }

        private void PopulateList(List<BackupItem> items, string destRoot)
        {
            foreach (var item in items)
            {
                string status = item.Enabled ? "ACTIVE" : "SKIPPED";
                string fromPath = item.Src;
                string toPath = System.IO.Path.Combine(destRoot, item.Dest);

                var row = new ListViewItem(status);
                row.SubItems.Add(item.Name);
                row.SubItems.Add(fromPath);
                row.SubItems.Add(toPath);

                // Color-code rows
                row.ForeColor = item.Enabled
                    ? System.Drawing.Color.DarkCyan
                    : System.Drawing.Color.Gray;

                _listView.Items.Add(row);
            }
        }
    }
}