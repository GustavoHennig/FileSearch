using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFileSearch
{
    public partial class MainWindow : Window
    {
        const int MaxHistorySize = 50;
        private CancellationTokenSource? _searchCancellationTokenSource;

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
            
            Settings.Current.MaxFileSize = int.TryParse(txtMaxFileSize.Text, out var maxSize) ? maxSize : 0;
            Settings.Current.ParallelSearches = int.TryParse(txtParallelSearches.Text, out var parallel) ? parallel : 1;
            Settings.Current.IgnoreAccentuation = chkIgnoreAccent.IsChecked.GetValueOrDefault();

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

            txtMaxFileSize.Text = Settings.Current.MaxFileSize?.ToString();
            txtParallelSearches.Text = Settings.Current.ParallelSearches?.ToString();
            chkIgnoreAccent.IsChecked = Settings.Current.IgnoreAccentuation;
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            
            progressBarSearching.IsVisible = true;
            btnSearch.IsEnabled = false;
            btnCancel.IsVisible = true;
            
            // Access selected ComboBox values
            string fileNamePatterns = cmbFileName.Text ?? "*";
            string searchText = cmbInFile.Text ?? "";
            string directoryPath = cmbPath.Text ?? "";
            bool caseSensitive = chkCaseSens.IsChecked.GetValueOrDefault();
            bool ignoreAccent = chkIgnoreAccent.IsChecked.GetValueOrDefault();
            int maxFileSize = int.TryParse(txtMaxFileSize.Text, out var size) ? size : 0;
            int parallelSearches = int.TryParse(txtParallelSearches.Text, out var parallel) ? parallel : 1;
            

            Stopwatch stopwatch = new Stopwatch();
            try
            {
                PrintStatus("Saving data...");
                SaveData();

                PrintStatus("Searching files...");
                FileSearcher2 fs = new FileSearcher2();
                lstFiles.ItemsSource = null;

                stopwatch.Start();

                List<FileInfo> files = await Task.Run(async () =>
                   {
                       return await fs.SearchFilesAsync(directoryPath, fileNamePatterns, searchText, caseSensitive, ignoreAccent, maxFileSize, parallelSearches, PrintStatus, _searchCancellationTokenSource.Token);
                   }, _searchCancellationTokenSource.Token);

                if (!_searchCancellationTokenSource.Token.IsCancellationRequested)
                {
                    PrintStatus("Listing files...");
                    lstFiles.ItemsSource = files.Take(10000).ToList();
                    PrintStatus($"Finished in {stopwatch.ElapsedMilliseconds}ms - Found {files.Count} files");
                }
                else
                {
                    PrintStatus("Search cancelled");
                }
            }
            catch (OperationCanceledException)
            {
                PrintStatus("Search cancelled");
            }
            catch (Exception ex)
            {
                PrintStatus($"Error: {ex.Message}");
            }
            finally
            {
                progressBarSearching.IsVisible = false;
                btnSearch.IsEnabled = true;
                btnCancel.IsVisible = false;
                stopwatch.Stop();
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _searchCancellationTokenSource?.Cancel();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCalculateTotalSize_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.ItemsSource is List<FileInfo> files)
            {
                long totalSize = files.Sum(f => f.Length);
                lblStatus.Text = $"Total size: {totalSize / 1024:N0} KB";
            }
        }

        private async void btnSearchFolder_Click(object sender, RoutedEventArgs e)
        {
            string? currentPath = cmbPath.Text;
            var options = new FolderPickerOpenOptions
            {
                SuggestedStartLocation = await this.StorageProvider.TryGetFolderFromPathAsync(currentPath)
            };
            var result = await this.StorageProvider.OpenFolderPickerAsync(options);
            if (result != null && result.Any())
            {
                cmbPath.Text = result.First().Path.LocalPath;
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

        private void RemoveFromFileNameHistory_Click(object? sender, RoutedEventArgs e)
        {
            RemoveFromHistory(cmbFileName, Settings.Current.FileNameHistory);
        }

        private void ClearFileNameHistory_Click(object? sender, RoutedEventArgs e)
        {
            ClearHistory(cmbFileName, Settings.Current.FileNameHistory);
        }

        private void RemoveFromInFileHistory_Click(object? sender, RoutedEventArgs e)
        {
            RemoveFromHistory(cmbInFile, Settings.Current.SearchInsideFiles);
        }

        private void ClearInFileHistory_Click(object? sender, RoutedEventArgs e)
        {
            ClearHistory(cmbInFile, Settings.Current.SearchInsideFiles);
        }

        private void RemoveFromPathHistory_Click(object? sender, RoutedEventArgs e)
        {
            RemoveFromHistory(cmbPath, Settings.Current.PathHistory);
        }

        private void ClearPathHistory_Click(object? sender, RoutedEventArgs e)
        {
            ClearHistory(cmbPath, Settings.Current.PathHistory);
        }

        private void RemoveFromHistory(AutoCompleteBox comboBox, IList<string> historyList)
        {
            string? text = comboBox.Text;
            if (!string.IsNullOrEmpty(text) && historyList.Contains(text))
            {
                historyList.Remove(text);
                comboBox.ItemsSource = null;
                comboBox.ItemsSource = historyList;
                comboBox.Text = "";
                Settings.Save();
            }
        }

        private void ClearHistory(AutoCompleteBox comboBox, IList<string> historyList)
        {
            historyList.Clear();
            comboBox.ItemsSource = null;
            comboBox.ItemsSource = historyList;
            comboBox.Text = "";
            Settings.Save();
        }
    }
}
