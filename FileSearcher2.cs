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
        private int _totalFiles;
        private int _processedCount;
        private const int WorkerCount = 8; // Adjust number of consumers

        public async Task<List<FileInfo>> SearchFilesAsync(
            string directoryPath, string filenamePatterns, string searchText, bool isCaseSensitive, bool ignoreAccents, int maxFileSize, int parallelSearches, Action<string> statusCallback, CancellationToken cancellationToken = default)
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
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        foreach (string file in Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
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
                    cancellationToken.ThrowIfCancellationRequested();
                    _fileQueue.Add(file);
                }
                _fileQueue.CompleteAdding();

                var consumers = Enumerable.Range(0, WorkerCount)
                    .Select(_ => Task.Run(() => ProcessFiles(searchInsideFiles, searchText, isCaseSensitive, ignoreAccents, maxFileSize, statusCallback, cancellationToken)))
                    .ToArray();

                await Task.WhenAll(consumers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return _matchingFiles;
        }

        private void ProcessFiles(bool searchInsideFiles, string searchText, bool isCaseSensitive, bool ignoreAccents, int maxFileSize, Action<string> statusCallback, CancellationToken cancellationToken)
        {
            foreach (var filePath in _fileQueue.GetConsumingEnumerable(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (searchInsideFiles)
                {
                    if (ContainsTextInFile(filePath, searchText, isCaseSensitive, ignoreAccents, maxFileSize))
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

        private bool ContainsTextInFile(string filePath, string searchText, bool isCaseSensitive, bool ignoreAccents, int maxFileSize)
        {
            try
            {
                if (maxFileSize > 0)
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > maxFileSize * 1024) // maxFileSize is in KB
                        return false;
                }

                StringComparison comparisonType = isCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

                using StreamReader reader = new StreamReader(filePath);
                string searchTextToCompare = ignoreAccents ? RemoveDiacritics(searchText) : searchText;
                
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        string lineToCompare = ignoreAccents ? RemoveDiacritics(line) : line;
                        if (lineToCompare.Contains(searchTextToCompare, comparisonType))
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
