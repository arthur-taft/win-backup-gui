/*
 * Win-Backup
 * BackupEngine.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
using win_backup.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Principal;

namespace win_backup.Helpers
{
    public static class BackupEngine
    {

        // Returns true if the app is currently running with admin rights
        public static bool IsElevated()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        // Relaunches the current app elevated, passing along an argument.
        // Returns true if the relaunch was started (so the caller can close itself).
        public static bool RelaunchElevated(string argument)
        {
            var exePath = Process.GetCurrentProcess().MainModule.FileName;

            var startInfo = new ProcessStartInfo(exePath)
            {
                UseShellExecute = true,   // required for the "runas" verb
                Verb = "runas", // this is what triggers UAC
                Arguments = argument
            };

            try
            {
                Process.Start(startInfo);
                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // User clicked "No" on the UAC prompt — Windows throws this
                return false;
            }
        }

        public static List<string> GetUserProfiles()
        {
            var users = new List<string>();
            string usersRoot = @"C:\Users";

            // These aren't real user accounts — skip them
            var systemProfiles = new[] { "Public", "Default", "Default User", "All Users", "defaultuser0" };

            if (!Directory.Exists(usersRoot)) return users;

            foreach (var dir in Directory.GetDirectories(usersRoot))
            {
                string name = Path.GetFileName(dir);

                if (systemProfiles.Contains(name, StringComparer.OrdinalIgnoreCase))
                    continue;

                // A real profile will have a Desktop or NTUSER.DAT — quick sanity check
                if (File.Exists(Path.Combine(dir, "NTUSER.DAT")) || Directory.Exists(Path.Combine(dir, "Desktop")))
                    users.Add(name);
            }

            return users;
        }

        // Expose robocopy log path
        public static readonly string TempLogPath = Path.Combine(Path.GetTempPath(), "robo_migration.log");

        // Scans ready drives, returns dictionary of display text -> Drive Info
        public static Dictionary<string, DriveInfo> GetDrives()
        {
            var driveMapping = new Dictionary<string, DriveInfo>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;
                if (drive.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase)) continue;

                string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "Local Volume" : drive.VolumeLabel;
                double freeGB = Math.Round(drive.AvailableFreeSpace / 1073741824.0, 1);
                double totalGB = Math.Round(drive.TotalSize / 1073741824.0, 1);

                string displayText = $"Drive {drive.Name[0]}: [{label}] ({freeGB} GB Free of {totalGB} GB) - {drive.DriveType}";
                driveMapping[displayText] = drive;
            }

            return driveMapping;
        }

        // Builds the list of backup items
        public static List<BackupItem> BuildBackupItems(string username)
        {
            var items = new List<BackupItem>();
            string sourceRoot = Path.Combine(@"C:\Users", username); // selected user, not current

            // Standard user folders
            foreach (var folder in new[] { "Desktop", "Documents", "Downloads", "Pictures", "Videos", "Music" })
            {
                string fullPath = Path.Combine(sourceRoot, folder);
                if (!Directory.Exists(fullPath)) continue; // skip folders that don't exist for this user

                items.Add(new BackupItem
                {
                    Name = folder,
                    Src = fullPath,
                    Dest = folder,
                    Enabled = true
                });
            }

            // Browser profiles
            // Note we build AppData paths manually from the user's root
            // because Environment.GetFolderPath() would give the CURRENT user's AppData
            string localAppData = Path.Combine(sourceRoot, @"AppData\Local");
            string roamingAppData = Path.Combine(sourceRoot, @"AppData\Roaming");

            items.AddRange(GetChromiumProfiles("Chrome",
                Path.Combine(localAppData, @"Google\Chrome\User Data")));

            items.AddRange(GetChromiumProfiles("Edge",
                Path.Combine(localAppData, @"Microsoft\Edge\User Data")));

            items.AddRange(GetFirefoxProfiles(
                Path.Combine(roamingAppData, @"Mozilla\Firefox\Profiles")));

            return items;
        }

        // Handles Chrome and Edge
        private static List<BackupItem> GetChromiumProfiles(string browserName, string userDataPath)
        {
            var profiles = new List<BackupItem>();
            if (!Directory.Exists(userDataPath)) return profiles;

            foreach (var dir in Directory.GetDirectories(userDataPath))
            {
                string folderName = Path.GetFileName(dir);
                if (folderName == "Default" || Regex.IsMatch(folderName, @"^Profile \d+$"))
                {
                    profiles.Add(new BackupItem
                    {
                        Name = $@"{browserName}\{folderName}",
                        Src = dir,
                        Dest = $@"{browserName}\{folderName}",
                        Enabled = true
                    });
                }
            }

            return profiles;
        }

        private static List<BackupItem> GetFirefoxProfiles(string firefoxPath)
        {
            var profiles = new List<BackupItem>();

            if (!Directory.Exists(firefoxPath)) return profiles;

            foreach (var dir in Directory.GetDirectories(firefoxPath))
            {
                string folderName = Path.GetFileName(dir);
                if (Regex.IsMatch(folderName, @"^[a-zA-Z0-9]+\.default-release$") ||
                    Regex.IsMatch(folderName, @"^[a-zA-Z0-9]+\.Profile \d+$"))
                {
                    profiles.Add(new BackupItem
                    {
                        Name = $@"Firefox\{folderName}",
                        Src = dir,
                        Dest = $@"Firefox\{folderName}",
                        Enabled = true
                    });
                }
            }

            return profiles;
        }

        public static List<LargeFileItem> ScanLargeFiles(string path)
        {
            var results = new List<LargeFileItem>();
            const long fiveGB = 5368709120L; // 5 * 1024

            try
            {
                foreach (var filePath in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    var info = new FileInfo(filePath);
                    if (info.Length > fiveGB)
                    {
                        double sizeGB = Math.Round(info.Length / 1073741824.0, 2);
                        results.Add(new LargeFileItem
                        {
                            DisplayName = $"[{sizeGB} GB]  {filePath}",
                            FileName = info.Name,
                            Enabled = true
                        });
                    }
                }
            }
            catch { } // Silently skip any inaccessible folders

            return results;
        }

        public static Dictionary<string, List<string>> RunBackup(
            List<BackupItem> items,
            string destRoot,
            Dictionary<string, List<string>> excludedFiles,
            IProgress<CopyProgress> progress)
        {
            int threads = Environment.ProcessorCount;
            string tempLog = TempLogPath;
            int totalEnabled = items.Count(i => i.Enabled);
            int completed = 0;
            var auditLogs = new Dictionary<string, List<string>>();

            foreach (var item in items)
            {
                // --- Skipped / not found ---
                if (!item.Enabled)
                {
                    progress.Report(new CopyProgress
                    { ItemName = item.Name, Status = "Skipped", ItemIndex = completed, TotalItems = totalEnabled });
                    continue;
                }

                if (!Directory.Exists(item.Src))
                {
                    progress.Report(new CopyProgress
                    { ItemName = item.Name, Status = "Not Found", ItemIndex = completed, TotalItems = totalEnabled });
                    completed++;
                    continue;
                }

                // --- Signal item is starting ---
                progress.Report(new CopyProgress
                { ItemName = item.Name, Status = "Copying", ItemIndex = completed, TotalItems = totalEnabled });

                // --- Create destination folder ---
                Directory.CreateDirectory(Path.Combine(destRoot, item.Dest));
                if (File.Exists(tempLog)) File.Delete(tempLog);

                // --- Build robocopy arguments ---
                var args = new List<string>
                {
                    $"\"{item.Src}\"",
                    $"\"{Path.Combine(destRoot, item.Dest)}\"",
                    "/MIR", "/W:1", "/R:1", "/J",
                    $"/MT:{threads}",
                    "/NP", "/NDL",
                    $"/UNILOG:\"{tempLog}\""
                };

                if (excludedFiles.ContainsKey(item.Name))
                {
                    args.Add("/XF");
                    foreach (var file in excludedFiles[item.Name])
                        args.Add($"\"{file}\"");
                }

                // --- Launch robocopy hidden ---
                var process = Process.Start(new ProcessStartInfo("robocopy.exe", string.Join(" ", args))
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                int lastLineCount = 0;

                // --- Poll the log while robocopy runs ---
                while (!process.HasExited)
                {
                    if (File.Exists(tempLog))
                    {
                        try
                        {
                            string lastLine;
                            using (var fs = new FileStream(tempLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var sr = new StreamReader(fs, System.Text.Encoding.Unicode))
                            {
                                var lines = sr.ReadToEnd()
                                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                lastLine = lines.LastOrDefault(l => !string.IsNullOrWhiteSpace(l));
                            }

                            if (lastLine != null)
                            {
                                string clean = lastLine.Trim();
                                if (clean.Length > 80) clean = clean.Substring(0, 77) + "...";

                                progress.Report(new CopyProgress
                                {
                                    ItemName = item.Name,
                                    CurrentFile = clean,
                                    Status = "Copying",
                                    ItemIndex = completed,
                                    TotalItems = totalEnabled
                                });
                            }
                        }
                        catch { }
                    }
                    Thread.Sleep(150);
                }

                // --- Item done ---
                completed++;
                progress.Report(new CopyProgress
                { ItemName = item.Name, Status = "Complete", ItemIndex = completed, TotalItems = totalEnabled });

                if (File.Exists(tempLog))
                    auditLogs[item.Name] = new List<string>(File.ReadAllLines(tempLog));
            }
            return auditLogs;
        }

        public static Dictionary<string, List<string>> VerifyLogs(Dictionary<string, List<string>> auditLogs)
        {
            var failures = new Dictionary<string, List<string>>();

            foreach (var kvp in auditLogs)
            {
                var log = kvp.Value;

                // Find the "Files :" summary line
                string fileLine = log.FirstOrDefault(l => l.TrimStart().StartsWith("Files :"));
                if (fileLine == null) continue;

                var parts = fileLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 7) continue;

                // parts[6] is the Failed count — same index your PS script reads
                if (parts[6] == "0") continue;

                var errorLines = log.Where(l => l.Contains("ERROR")).ToList();

                if (!errorLines.Any())
                    errorLines.Add("Exact file names could not be parsed from the log output.");

                failures[kvp.Key] = errorLines;
            }

            return failures;
        }
    }
}
