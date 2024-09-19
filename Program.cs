using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace SimpleFileSearch
{
    static class Program
    {



        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            SetRegistryShellOption();
            Settings.Load();
            if (args.Length > 0)
            {
                string path = args[0];
                if (Directory.Exists(path))
                {
                    Settings.Current.CurrentDirectory = path;
                }
                else
                {
                    Settings.Current.CurrentDirectory = Path.GetDirectoryName(path);
                }
                Settings.Save();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
        private static void SetRegistryShellOption()
        {
            try
            {
                Registry.SetValue("HKEY_CLASSES_ROOT\\Directory\\shell\\GH Software FileSearch\\command", "", "\"" + Application.ExecutablePath + "\" \"%1\"");
            }
            catch (Exception)
            {
            }
        }
    }
}