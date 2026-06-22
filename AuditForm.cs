/*
 * Win-Backup
 * AuditForm.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace win_backup
{
    public class AuditForm : Form
    {
        private RichTextBox _rtbResults;
        private Button _btnSaveReport;
        private Button _btnClose;

        private readonly Dictionary<string, List<string>> _failures;

        public AuditForm(Dictionary<string, List<string>> failures)
        {
            _failures = failures;
            SetupForm();
            PopulateResults();
        }

        private void SetupForm()
        {
            Text = "Post-Copy Audit Results";
            Size = new Size(700, 500);
            MinimumSize = new Size(500, 350);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            _rtbResults = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            _btnSaveReport = new Button
            {
                Text = "Save Failure Report",
                Size = new Size(155, 35),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Location = new Point(10, 8),
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Visible = false
            };
            _btnSaveReport.Click += BtnSaveReport_Click;

            _btnClose = new Button { Text = "Close", Size = new Size(100, 35) };
            _btnClose.Click += (s, e) => Close();

            bottomPanel.Controls.Add(_btnSaveReport);
            bottomPanel.Controls.Add(_btnClose);
            bottomPanel.Resize += (s, e) =>
            {
                _btnClose.Left = (bottomPanel.Width - _btnClose.Width) / 2;
                _btnClose.Top = (bottomPanel.Height - _btnClose.Height) / 2;
            };

            Controls.Add(_rtbResults);
            Controls.Add(bottomPanel);
        }

        private void PopulateResults()
        {
            if (_failures.Count == 0)
            {
                AppendColored("  ✓ All transfers verified — no failures detected.\n", Color.LimeGreen);
                return;
            }

            AppendColored($"  ⚠ {_failures.Count} location(s) had transfer failures:\n\n", Color.OrangeRed);

            foreach (var kvp in _failures)
            {
                AppendColored($"  LOCATION: {kvp.Key}\n", Color.Red);
                foreach (var line in kvp.Value)
                {
                    string clean = Regex.Replace(line,
                        @"^\s*\d{4}/\d{2}/\d{2}\s+\d{2}:\d{2}:\d{2}\s+", "").Trim();
                    AppendColored($"    → {clean}\n", Color.Silver);
                }
                AppendColored("\n", Color.White);
            }

            _btnSaveReport.Visible = true;
        }

        private void AppendColored(string text, Color color)
        {
            _rtbResults.SelectionStart = _rtbResults.TextLength;
            _rtbResults.SelectionLength = 0;
            _rtbResults.SelectionColor = color;
            _rtbResults.AppendText(text);
        }

        private void BtnSaveReport_Click(object sender, EventArgs e)
        {
            string reportPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Migration_Failed_Files_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            var lines = new List<string>();
            foreach (var kvp in _failures)
            {
                lines.Add($"LOCATION: {kvp.Key}");
                lines.Add("---------------------------------------------------");
                foreach (var errLine in kvp.Value)
                {
                    string clean = Regex.Replace(errLine,
                        @"^\s*\d{4}/\d{2}/\d{2}\s+\d{2}:\d{2}:\d{2}\s+", "").Trim();
                    lines.Add(clean);
                }
                lines.Add("");
            }

            File.WriteAllLines(reportPath, lines);
            MessageBox.Show($"Report saved to:\n{reportPath}", "Report Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
