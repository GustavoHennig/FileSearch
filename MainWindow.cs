using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace SimpleFileSearch
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            this.Text += " - " + Application.ProductVersion;
        }

        private void SaveData()
        {
            const int tammax = 50;

            if (!Settings.Current.FileNameHistory.Contains(cmbFileName.Text))
            {
                Settings.Current.FileNameHistory.Insert(0, cmbFileName.Text);
                cmbFileName.Items.Insert(0, cmbFileName.Text);
                if (Settings.Current.FileNameHistory.Count > tammax)
                {
                    Settings.Current.FileNameHistory.RemoveAt(tammax - 1);
                }
            }

            if (!Settings.Current.SearchInsideFiles.Contains(cmbInFile.Text))
            {
                Settings.Current.SearchInsideFiles.Insert(0, cmbInFile.Text);
                cmbInFile.Items.Insert(0, cmbInFile.Text);
                if (Settings.Current.SearchInsideFiles.Count > tammax)
                {
                    Settings.Current.SearchInsideFiles.RemoveAt(tammax - 1);
                }
            }
            if (!Settings.Current.PathHistory.Contains(cmbPath.Text))
            {
                Settings.Current.PathHistory.Insert(0, cmbPath.Text);
                cmbPath.Items.Insert(0, cmbPath.Text);
                if (Settings.Current.PathHistory.Count > tammax)
                {
                    Settings.Current.PathHistory.RemoveAt(tammax - 1);
                }
            }

            Settings.Save();
        }

        private void LoadData()
        {
            foreach (string s in Settings.Current.FileNameHistory)
            {
                cmbFileName.Items.Add(s);
            }

            foreach (string s in Settings.Current.SearchInsideFiles)
            {
                cmbInFile.Items.Add(s);
            }
            foreach (string s in Settings.Current.PathHistory)
            {
                cmbPath.Items.Add(s);
            }

            cmbPath.Text = Settings.Current.CurrentDirectory;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            PrintStatus("Saving data...");
            SaveData();
            lstFiles.Items.Clear();
            PrintStatus("Searching files...");
            FileSearcher fs = new FileSearcher();
            fs.estado += new Estado(fs_estado);

            List<FileInfo> files = fs.SearchFiles(cmbPath.Text, cmbFileName.Text, cmbInFile.Text, chkCaseSens.Checked);

            PrintStatus("Listing files...");

            int limit = 2000;
            foreach (FileInfo f in files)
            {
                lstFiles.Items.Add(f);
                limit--;
                if (limit < 0)
                    break;
            }

            PrintStatus("Ready.");
        }

        void fs_estado(string value)
        {
            PrintStatus(value);
        }

        private void lstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItem != null)
            {
                FileInfo f = (FileInfo)lstFiles.SelectedItem;
                Process.Start(Environment.GetEnvironmentVariable("Windir") + "\\" + "explorer.exe", f.Directory.FullName);
            }
        }

        private void PrintStatus(string status)
        {
            lblEstado.Text = status;
            Application.DoEvents();
            lblEstado.Refresh();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/GustavoHennig/FileSearch");
            }
            catch (Exception)
            {
            }
        }

        private void btnSearchFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.SelectedPath = cmbPath.Text;
            if (f.ShowDialog() == DialogResult.OK)
            {
                cmbPath.Text = f.SelectedPath;
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Settings.Default.Save();
        }
    }
}