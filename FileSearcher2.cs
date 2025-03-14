using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFileSearch
{
    public class FileSearcher2
    {
        private  BlockingCollection<string> _fileQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
        private readonly List<FileInfo> _matchingFiles = new List<FileInfo>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _totalFiles;
        private int _processedCount;
        private const int WorkerCount = 8; // Adjust number of consumers

        public async Task<List<FileInfo>> SearchFilesAsync(string directoryPath, string filenamePatterns, string searchText, bool isCaseSensitive, bool ignoreAccents, Action<string> statusCallback)
        {
            bool searchInsideFiles = !string.IsNullOrEmpty(searchText);
            _processedCount = 0;
            _matchingFiles.Clear();
            _fileQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());

            try
            {
                string[] patterns = filenamePatterns.Split(';', StringSplitOptions.RemoveEmptyEntries);
                HashSet<string> allFiles = new HashSet<string>();

                foreach (string pattern in patterns)
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories))
                        {
                            allFiles.Add(file);
                        }
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (DirectoryNotFoundException) { }
                }

                _totalFiles = allFiles.Count;
                statusCallback?.Invoke($"Found {_totalFiles} files to check...");

                foreach (var file in allFiles)
                {
                    _fileQueue.Add(file);
                }
                _fileQueue.CompleteAdding();

                var consumers = Enumerable.Range(0, WorkerCount)
                    .Select(_ => Task.Run(() => ProcessFiles(searchInsideFiles, searchText, isCaseSensitive, ignoreAccents, statusCallback, _cts.Token)))
                    .ToArray();

                await Task.WhenAll(consumers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _cts.Cancel();
            }

            return _matchingFiles;
        }

        private void ProcessFiles(bool searchInsideFiles, string searchText, bool isCaseSensitive, bool ignoreAccents, Action<string> statusCallback, CancellationToken cancellationToken)
        {
            foreach (var filePath in _fileQueue.GetConsumingEnumerable(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (searchInsideFiles)
                {
                    if (ContainsTextInFile(filePath, searchText, isCaseSensitive, ignoreAccents))
                    {
                        lock (_matchingFiles)
                        {
                            _matchingFiles.Add(new FileInfo(filePath));
                        }
                    }
                }
                else
                {
                    lock (_matchingFiles)
                    {
                        _matchingFiles.Add(new FileInfo(filePath));
                    }
                }

                Interlocked.Increment(ref _processedCount);
                statusCallback?.Invoke($"Processed {_processedCount} of {_totalFiles} files...");
            }
        }

        private bool ContainsTextInFile(string filePath, string searchText, bool isCaseSensitive, bool ignoreAccents)
        {
            try
            {
                StringComparison comparisonType = isCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

                using StreamReader reader = new StreamReader(filePath);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        string lineToCompare = ignoreAccents ? RemoveDiacritics(line) : line;
                        if (lineToCompare.Contains(searchText, comparisonType))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return false;
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            try
            {
                return string.Concat(text
                    .Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
                    .Normalize(NormalizationForm.FormC);
            }
            catch (Exception)
            {
                return text;
            }
        }
    }
}
