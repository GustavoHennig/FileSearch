using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Avalonia;
using System.Reflection;
using System.Diagnostics;

namespace SimpleFileSearch
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
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

            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
        }

        private static void SetRegistryShellOption()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var fileName = Process.GetCurrentProcess().MainModule.FileName;
                    Registry.SetValue($"HKEY_CLASSES_ROOT\\Directory\\shell\\GH Software FileSearch\\command", "", $"\"{fileName}\" \"%1\"");
                }
            }
            catch (Exception)
            {
            }
        }
    }
}