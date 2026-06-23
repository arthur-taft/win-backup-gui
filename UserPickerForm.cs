using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace win_backup
{
    public class UserPickerForm : Form
    {
        private ListBox _list;
        public string SelectedUser { get; private set; }

        public UserPickerForm(List<string> users, string currentUser)
        {
            Text = "Select User to Back Up";
            Size = new Size(340, 380);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var lbl = new Label
            {
                Text = "Choose which user profile to back up:",
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(10, 8, 0, 0)
            };

            _list = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            foreach (var u in users) _list.Items.Add(u);
            _list.SelectedItem = currentUser; // preselect whoever's active now

            var panel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            var ok = new Button { Text = "Select", Size = new Size(90, 32), Location = new Point(60, 9) };
            var cancel = new Button { Text = "Cancel", Size = new Size(90, 32), Location = new Point(170, 9) };

            ok.Click += (s, e) =>
            {
                if (_list.SelectedItem == null) return;
                SelectedUser = _list.SelectedItem.ToString();
                DialogResult = DialogResult.OK;
                Close();
            };
            cancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            panel.Controls.Add(ok);
            panel.Controls.Add(cancel);

            Controls.Add(_list);
            Controls.Add(lbl);
            Controls.Add(panel);
        }
    }
}