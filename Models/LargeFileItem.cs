/*
 * Win-Backup
 * LargeFileItem.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
namespace win_backup.Models
{
    public class LargeFileItem
    {
        public string DisplayName { get; set; } // "[6.2 GB] C:\Users\...\bigfile.mp4"
        public string FileName { get; set; } // "bigfile.mp4" — used for robocopy /XF flag
        public bool Enabled { get; set; } = true;
    }
}
