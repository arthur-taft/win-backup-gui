namespace win_backup.Models
{
    public class CopyProgress
    {
        public string ItemName { get; set; } // Which folder to back up
        public string CurrentFile { get; set; } // Latest file from the robocopy log
        public string Status { get; set; } // "Copying", "Complete", "Skipped", "Not Found"
        public int ItemIndex { get; set; } // How many items finished (for progress bar)
        public int TotalItems { get; set; } // Total enabled items
    }
}