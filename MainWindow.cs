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
using FileSearch.Properties;

namespace FileSearching
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

            if (!Program.cgf.SugFileNames.Contains(cmbFileName.Text))
            {
                Program.cgf.SugFileNames.Insert(0, cmbFileName.Text);
                cmbFileName.Items.Insert(0, cmbFileName.Text);
                if (Program.cgf.SugFileNames.Count > tammax)
                {
                    Program.cgf.SugFileNames.RemoveAt(tammax - 1);
                }
            }

            if (!Program.cgf.SugInFiles.Contains(cmbInFile.Text))
            {
                Program.cgf.SugInFiles.Insert(0, cmbInFile.Text);
                cmbInFile.Items.Insert(0, cmbInFile.Text);
                if (Program.cgf.SugInFiles.Count > tammax)
                {
                    Program.cgf.SugInFiles.RemoveAt(tammax - 1);
                }
            }
            if (!Program.cgf.SugPaths.Contains(cmbPath.Text))
            {
                Program.cgf.SugPaths.Insert(0, cmbPath.Text);
                cmbPath.Items.Insert(0, cmbPath.Text);
                if (Program.cgf.SugPaths.Count > tammax)
                {
                    Program.cgf.SugPaths.RemoveAt(tammax - 1);
                }
            }

            Serializer s = new Serializer();
            s.Grava(Program.cgf, Program.ConfigFile);

        }

        private void LoadData()
        {

            //Settings.Default.FileNameHistory.ToString();

            foreach (string s in Program.cgf.SugFileNames)
            {
                cmbFileName.Items.Add(s);
            }

            foreach (string s in Program.cgf.SugInFiles)
            {
                cmbInFile.Items.Add(s);
            }
            foreach (string s in Program.cgf.SugPaths)
            {
                cmbPath.Items.Add(s);
            }

            cmbPath.Text = Program.cgf.arg;

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
            //if (files.Count > 2000)
            //{
            //    MessageBox.Show("There are more than 2000 files founded. They won't be displayed.");
            //}
            //else
            //{
            foreach (FileInfo f in files)
            {
                lstFiles.Items.Add(f);
                limit--;
                if (limit < 0)
                    break;
            }
            //}

            PrintStatus("Ready.");

        }

        void fs_estado(string value)
        {
            PrintStatus(value);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

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
            Settings.Default.Save();
        }
    }
}