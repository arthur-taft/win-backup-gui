using win_backup.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace win_backup
{
    public class LogForm : Form
    {
        private RichTextBox _rtbLog;
        private System.Windows.Forms.Timer _timer;
        private int _lastLineCount = 0;

        public LogForm()
        {
            SetupForm();
        }

        private void SetupForm()
        {
            Text = "Live Robocopy Log";
            Size = new Size(800, 500);
            MinimumSize = new Size(500, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            _rtbLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(12, 12, 12),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 45 };
            var btnClear = new Button { Text = "Clear", Size = new Size(80, 28) };
            var btnClose = new Button { Text = "Close", Size = new Size(80, 28) };

            btnClear.Click += (s, e) =>
            {
                _rtbLog.Clear();
                _lastLineCount = 0; // re-read from top of file
            };
            btnClose.Click += (s, e) => Close();

            bottomPanel.Controls.Add(btnClear);
            bottomPanel.Controls.Add(btnClose);
            bottomPanel.Resize += (s, e) =>
            {
                int totalWidth = btnClear.Width + 10 + btnClose.Width;
                int startX = (bottomPanel.Width - totalWidth) / 2;
                int centreY = (bottomPanel.Height - btnClear.Height) / 2;
                btnClear.Location = new Point(startX, centreY);
                btnClose.Location = new Point(startX + btnClear.Width + 10, centreY);
            };

            Controls.Add(_rtbLog);
            Controls.Add(bottomPanel);

            // Timer tails the log file every 200ms on the UI thread — no Invoke needed
            _timer = new System.Windows.Forms.Timer { Interval = 200 };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            FormClosed += (s, e) => _timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!File.Exists(BackupEngine.TempLogPath)) return;

            try
            {
                List<string> allLines;

                // FileShare.ReadWrite lets us read while robocopy still has the file open
                using (var fs = new FileStream(BackupEngine.TempLogPath,
                           FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.Unicode))
                {
                    allLines = sr.ReadToEnd()
                        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                        .ToList();
                }

                if (allLines.Count <= _lastLineCount) return;

                var newLines = allLines.Skip(_lastLineCount).ToList();
                _lastLineCount = allLines.Count;

                foreach (var line in newLines.Where(l => !string.IsNullOrWhiteSpace(l)))
                    _rtbLog.AppendText(line.TrimEnd() + "\n");

                _rtbLog.SelectionStart = _rtbLog.TextLength;
                _rtbLog.ScrollToCaret();
            }
            catch { }
        }
    }
}