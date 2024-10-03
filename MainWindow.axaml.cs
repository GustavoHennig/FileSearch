using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleFileSearch
{
    public partial class MainWindow : Window
    {
        const int MaxHistorySize = 50;

        public MainWindow()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
                return;

            LoadData();
            this.Title += " - " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Closing += this.MainWindow_Closing; ;
        }


        private void SaveData()
        {
            UpdateHistory(cmbFileName, Settings.Current.FileNameHistory);
            UpdateHistory(cmbInFile, Settings.Current.SearchInsideFiles);
            UpdateHistory(cmbPath, Settings.Current.PathHistory);
            Settings.Current.MainWindowWidth = this.Width;
            Settings.Current.MainWindowHeight = this.Height;
            Settings.Current.SplitContainer1Panel1Width = (int)gridMain.ColumnDefinitions[0].Width.Value;
            Settings.Current.SplitContainer1Panel2Width = (int)gridMain.ColumnDefinitions[2].Width.Value;
            Settings.Save();
        }

        private void UpdateHistory(AutoCompleteBox comboBox, IList<string> historyList)
        {
            string? text = comboBox.Text;

            if (text is not null)
            {
                historyList.Remove(text);
                historyList.Insert(0, text);
                comboBox.ItemsSource = null; // Reset ItemsSource to refresh the binding
                comboBox.ItemsSource = historyList;

                while (historyList.Count > MaxHistorySize)
                {
                    historyList.RemoveAt(historyList.Count - 1); // Remove the oldest entry
                }
            }
        }


        private void LoadData()
        {

            if (Settings.Current.MainWindowWidth > 0 && Settings.Current.MainWindowHeight > 0)
            {
                this.Width = Settings.Current.MainWindowWidth;
                this.Height = Settings.Current.MainWindowHeight;
            }
            gridMain.ColumnDefinitions[0].Width = new GridLength(Settings.Current.SplitContainer1Panel1Width);
            //        gridMain.ColumnDefinitions[2].Width = new GridLength(Settings.Current.SplitContainer1Panel2Width);


            cmbFileName.ItemsSource = Settings.Current.FileNameHistory;
            cmbFileName.SelectedItem = Settings.Current.FileNameHistory.FirstOrDefault();

            cmbInFile.ItemsSource = Settings.Current.SearchInsideFiles;
            cmbInFile.SelectedItem = Settings.Current.SearchInsideFiles.FirstOrDefault();

            cmbPath.ItemsSource = Settings.Current.PathHistory;
            if (Settings.Current.CurrentDirectory is null)
            {
                cmbPath.SelectedItem = Settings.Current.PathHistory.FirstOrDefault();
            }
            else
            {
                cmbPath.Text = Settings.Current.CurrentDirectory;
            }
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            progressBarSearching.IsVisible = true;
            // Access selected ComboBox values
            string fileName = cmbFileName.Text;
            string inFile = cmbInFile.Text;
            string path = cmbPath.Text;
            bool caseSensitive = chkCaseSens.IsChecked.GetValueOrDefault();


            try
            {
                PrintStatus("Saving data...");
                SaveData();

                PrintStatus("Searching files...");
                FileSearcher fs = new FileSearcher();
                fs.estado += PrintStatus;

                List<FileInfo> files = await Task.Run(() =>
                   {
                       return fs.SearchFiles(path, fileName, inFile, caseSensitive);
                   });

                PrintStatus("Listing files...");

                foreach (var file in files.Take(2000))
                {
                    lstFiles.ItemsSource = files; // Bind found files to ListBox
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                progressBarSearching.IsVisible = false;
                PrintStatus("Ready.");
            }
        }

        private void lstFiles_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (lstFiles.SelectedItem is FileInfo f)
            {
                ProcessStart(f.FullName);
            }
        }

        private void PrintStatus(string status)
        {
            Dispatcher.UIThread.Invoke(() =>
           {
               lblStatus.Text = status;
           });

        }

        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            SaveData();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
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
        private void ProcessStart(string fullFilePath)
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{fullFilePath}\"") { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", fullFilePath);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", fullFilePath);
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

        private void lblAppInfo_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            try
            {
                OpenUrl("https://github.com/GustavoHennig/FileSearch");
            }
            catch (Exception)
            {
            }
        }
        private void AutoCompleteBox_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            if (sender is AutoCompleteBox autoCompleteBox)
            {
                // Open the dropdown to show suggestions
                autoCompleteBox.IsDropDownOpen = true;
            }
        }
    }
}
