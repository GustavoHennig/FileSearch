using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SimpleFileSearch
{
    public partial class MainWindowAvalonia : Window
    {

        public MainWindowAvalonia()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
                return;

            LoadData();
            this.Title += " - " + "1.0"; // Replace with Application.ProductVersion equivalent


             
        }

         private void SaveData()
        {
            const int tammax = 50;

            if (!Settings.Current.FileNameHistory.Contains(cmbFileName.Text))
            {
                Settings.Current.FileNameHistory.Insert(0, cmbFileName.Text);
                cmbFileName.ItemsSource = Settings.Current.FileNameHistory; // Update ItemsSource
                if (Settings.Current.FileNameHistory.Count > tammax)
                {
                    Settings.Current.FileNameHistory.RemoveAt(tammax - 1);
                }
            }

            if (!Settings.Current.SearchInsideFiles.Contains(cmbInFile.Text))
            {
                Settings.Current.SearchInsideFiles.Insert(0, cmbInFile.Text);
                cmbInFile.ItemsSource = Settings.Current.SearchInsideFiles;
                if (Settings.Current.SearchInsideFiles.Count > tammax)
                {
                    Settings.Current.SearchInsideFiles.RemoveAt(tammax - 1);
                }
            }

            if (!Settings.Current.PathHistory.Contains(cmbPath.Text))
            {
                Settings.Current.PathHistory.Insert(0, cmbPath.Text);
                cmbPath.ItemsSource = Settings.Current.PathHistory;
                if (Settings.Current.PathHistory.Count > tammax)
                {
                    Settings.Current.PathHistory.RemoveAt(tammax - 1);
                }
            }

            Settings.Save();
        }

        private void LoadData()
        {
            cmbFileName.ItemsSource = Settings.Current.FileNameHistory;
            cmbInFile.ItemsSource = Settings.Current.SearchInsideFiles;
            cmbPath.ItemsSource = Settings.Current.PathHistory;
            cmbPath.Text = Settings.Current.CurrentDirectory;
        }
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Access selected ComboBox values
            string fileName = cmbFileName.Text;
            string inFile = cmbInFile.Text;
            string path = cmbPath.Text;

            PrintStatus("Saving data...");
            SaveData();

            PrintStatus("Searching files...");
            FileSearcher fs = new FileSearcher();
            fs.estado += fs_estado;

            List<FileInfo> files = fs.SearchFiles(path, fileName, inFile, chkCaseSens.IsChecked.GetValueOrDefault());

            PrintStatus("Listing files...");
            foreach (var file in files.Take(2000))
            {
                lstFiles.ItemsSource = files; // Bind found files to ListBox
            }

            PrintStatus("Ready.");
        }
        private void fs_estado(string value)
        {
            PrintStatus(value);
        }

        private void lstFiles_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItem is FileInfo f)
            {
                Process.Start(new ProcessStartInfo("explorer.exe", f.Directory.FullName) { UseShellExecute = true });
            }
        }

        private void PrintStatus(string status)
        {
            lblEstado.Text = status;
            lblEstado.UpdateLayout();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSearchFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync(this);
            if (result != null)
            {
                cmbPath.Text = result;
            }
        }

        private void lblStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenUrl("https://github.com/GustavoHennig/FileSearch") ;
            }
            catch (Exception)
            {
            }
        }

        
        private void ProcessStart(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", path);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", path);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", path);
            }


        }
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {

            }
        }

    }
}
