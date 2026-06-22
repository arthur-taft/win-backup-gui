/*
 * Win-Backup
 * BackupItem.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace win_backup.Models
{
    public class BackupItem
    {
        public string Name { get; set; }
        public string Src { get; set; }
        public string Dest { get; set; }
        public bool Enabled { get; set; }
    }
}
