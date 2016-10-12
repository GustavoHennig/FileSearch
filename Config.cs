using System;
using System.Collections.Generic;
using System.Text;

namespace FileSearching
{
    [Serializable]
    public class Config
    {
        public List<string> SugFileNames = new List<string>();
        public List<string> SugPaths = new List<string>();
        public List<string> SugInFiles = new List<string>();
        public string arg;

    }
}
