using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSearch
{
    public class SettingsContent
    {
        public List<string> FileNameHistory { get; set; } = new List<string>();
        public List<string> PathHistory { get; set; } = new List<string>();
        public List<string> SearchInsideFiles { get; set; } = new List<string>();
        public string CurrentDirectory { get; internal set; }
    }
}
