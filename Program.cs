using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace FileSearching
{
    static class Program
    {

        public static Config cgf = new Config();
        public static string ConfigFile;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ConfigFile = Path.GetDirectoryName(Application.ExecutablePath) + "\\config.xml";

            VerificaRegistro();

            if (File.Exists(ConfigFile))
            {
                Serializer s = new Serializer();
                cgf = (Config)s.Busca(ConfigFile, typeof(Config));
            }
            if (args.Length > 0)
            {
                cgf.arg = args[0];
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
        private static void VerificaRegistro()
        {
            try
            {
                Registry.SetValue("HKEY_CLASSES_ROOT\\Directory\\shell\\Neliware FileSearch\\command", "", "\"" + Application.ExecutablePath + "\" \"%1\"");
            }
            catch (Exception)
            {
            }
        }
    }
}