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
        public string CurrentDirectory { get;  set; }
        public int SplitContainer1Panel1Width { get;  set; }
        public int SplitContainer1Panel2Width { get;  set; }
        public double MainWindowWidth { get;  set; }
        public double MainWindowHeight { get;  set; }
        public int? MaxFileSize { get; internal set; }
        public int? ParallelSearches { get; internal set; }
        public bool? IgnoreAccentuation { get; internal set; }
    }
}
