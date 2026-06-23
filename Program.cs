/*
 * Win-Backup
 * Program.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
namespace win_backup
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(args));
        }
    }
}
