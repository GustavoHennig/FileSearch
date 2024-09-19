using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleFileSearch
{
    public class Settings
    {
        public static SettingsContent Current { get; set; }



        public static void Load()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleFileSearch");
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            string settingsFilePath = Path.Combine(appDataFolder, "settings.json");
            if (File.Exists(settingsFilePath))
            {
                string settingsJson = File.ReadAllText(settingsFilePath);
                Current = JsonSerializer.Deserialize<SettingsContent>(settingsJson);
            }
            else
            {
                Current = new SettingsContent();
            }
        }
        public static void Save()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleFileSearch");
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            string settingsFilePath = Path.Combine(appDataFolder, "settings.json");
            string settingsJson = JsonSerializer.Serialize(Current);
            File.WriteAllText(settingsFilePath, settingsJson);
        }
    }
}
