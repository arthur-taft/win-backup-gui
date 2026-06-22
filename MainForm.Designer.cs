/*
 * Win-Backup
 * MainForm.Designer.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
namespace win_backup
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label2 = new Label();
            cboDrive = new ComboBox();
            groupBox1 = new GroupBox();
            clbLocations = new CheckedListBox();
            groupBox2 = new GroupBox();
            btnPreview = new Button();
            chkSizeCheck = new CheckBox();
            chkLargeFileAudit = new CheckBox();
            btnStart = new Button();
            btnExit = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(12, 15);
            label2.Name = "label2";
            label2.Size = new Size(108, 15);
            label2.TabIndex = 0;
            label2.Text = "Destination Drive:";
            // 
            // cboDrive
            // 
            cboDrive.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDrive.FormattingEnabled = true;
            cboDrive.Location = new Point(12, 35);
            cboDrive.Name = "cboDrive";
            cboDrive.Size = new Size(555, 23);
            cboDrive.TabIndex = 1;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(clbLocations);
            groupBox1.Location = new Point(12, 75);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(310, 360);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Backup Locations";
            // 
            // clbLocations
            // 
            clbLocations.CheckOnClick = true;
            clbLocations.FormattingEnabled = true;
            clbLocations.Location = new Point(10, 25);
            clbLocations.Name = "clbLocations";
            clbLocations.Size = new Size(288, 310);
            clbLocations.TabIndex = 0;
            clbLocations.ItemCheck += clbLocations_ItemCheck;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnPreview);
            groupBox2.Controls.Add(chkSizeCheck);
            groupBox2.Controls.Add(chkLargeFileAudit);
            groupBox2.Location = new Point(335, 75);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(232, 360);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Options";
            // 
            // btnPreview
            // 
            btnPreview.Location = new Point(12, 110);
            btnPreview.Name = "btnPreview";
            btnPreview.Size = new Size(200, 35);
            btnPreview.TabIndex = 2;
            btnPreview.Text = "Preview Paths";
            btnPreview.UseVisualStyleBackColor = true;
            btnPreview.Click += btnPreview_Click;
            // 
            // chkSizeCheck
            // 
            chkSizeCheck.AutoSize = true;
            chkSizeCheck.Checked = true;
            chkSizeCheck.CheckState = CheckState.Checked;
            chkSizeCheck.Location = new Point(12, 60);
            chkSizeCheck.Name = "chkSizeCheck";
            chkSizeCheck.Size = new Size(137, 19);
            chkSizeCheck.TabIndex = 1;
            chkSizeCheck.Text = "Post-Copy Size Audit";
            chkSizeCheck.UseVisualStyleBackColor = true;
            // 
            // chkLargeFileAudit
            // 
            chkLargeFileAudit.AutoSize = true;
            chkLargeFileAudit.Checked = true;
            chkLargeFileAudit.CheckState = CheckState.Checked;
            chkLargeFileAudit.Location = new Point(12, 30);
            chkLargeFileAudit.Name = "chkLargeFileAudit";
            chkLargeFileAudit.Size = new Size(148, 19);
            chkLargeFileAudit.TabIndex = 0;
            chkLargeFileAudit.Text = "Large File Audit (5GB+)";
            chkLargeFileAudit.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.Green;
            btnStart.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStart.ForeColor = Color.White;
            btnStart.Location = new Point(12, 455);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(460, 40);
            btnStart.TabIndex = 4;
            btnStart.Text = "Start Backup";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += this.btnStart_Click;
            // 
            // btnExit
            // 
            btnExit.Location = new Point(485, 455);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(82, 40);
            btnExit.TabIndex = 5;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // MainForm
            // 
            ClientSize = new Size(584, 501);
            Controls.Add(btnExit);
            Controls.Add(btnStart);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(cboDrive);
            Controls.Add(label2);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Backup Utility";
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Label label2;
        private ComboBox cboDrive;
        private GroupBox groupBox1;
        private CheckedListBox clbLocations;
        private GroupBox groupBox2;
        private Button btnPreview;
        private CheckBox chkSizeCheck;
        private CheckBox chkLargeFileAudit;
        private Button btnStart;
        private Button btnExit;
    }
}
