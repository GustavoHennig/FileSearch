using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileSearch
{
    /// <summary>
    /// Provides functionality for searching files by name and optionally searching for text within files.
    /// The search can be case-sensitive and/or accent-insensitive.
    /// </summary>
    public class FileSearcherAsync
    {
        /// <summary>
        /// Searches for files in the specified directory based on the given filename patterns.
        /// Optionally, it searches for specific text within the found files.
        /// </summary>
        /// <param name="directoryPath">The root directory to search.</param>
        /// <param name="filenamePatterns">File name patterns separated by ';' (e.g., "*.txt;*.log").</param>
        /// <param name="searchText">Text to search for inside the files (optional).</param>
        /// <param name="isCaseSensitive">Determines if text search should be case-sensitive.</param>
        /// <param name="ignoreAccents">Determines if accentuation should be ignored in the search.</param>
        /// <param name="statusCallback">Callback function to report status updates.</param>
        /// <returns>A list of matching <see cref="FileInfo"/> objects.</returns>
        public async Task<List<FileInfo>> SearchFilesAsync(string directoryPath, string filenamePatterns, string searchText, bool isCaseSensitive, bool ignoreAccents, Action<string> statusCallback)
        {
            List<FileInfo> matchingFiles = new List<FileInfo>();
            bool searchInsideFiles = !string.IsNullOrEmpty(searchText);

            try
            {
                // Split filename patterns by ';' and remove empty entries
                string[] patterns = filenamePatterns.Split(';', StringSplitOptions.RemoveEmptyEntries);
                HashSet<string> allFiles = new HashSet<string>();

                // Collect all matching files based on patterns
                foreach (string pattern in patterns)
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories))
                        {
                            allFiles.Add(file);
                        }
                    }
                    catch (UnauthorizedAccessException) { /* Skip folders without permission */ }
                    catch (DirectoryNotFoundException) { /* Skip missing folders */ }
                }

                // Initial status update
                statusCallback?.Invoke("Verifying files...");

                int totalFiles = allFiles.Count;
                int processedCount = 0;
                long lastUpdateTime = Environment.TickCount64;
                const long updateIntervalMs = 500; // UI update interval in milliseconds

                // Iterate through all files found
                foreach (string filePath in allFiles)
                {
                    processedCount++;

                    // Check if the file contains the search text (if applicable)
                    if (searchInsideFiles)
                    {
                        if (await ContainsTextInFileAsync(filePath, searchText, isCaseSensitive, ignoreAccents))
                        {
                            matchingFiles.Add(new FileInfo(filePath));
                        }
                    }
                    else
                    {
                        matchingFiles.Add(new FileInfo(filePath));
                    }

                    // Limit UI updates to avoid excessive updates
                    long currentTime = Environment.TickCount64;
                    if (currentTime - lastUpdateTime >= updateIntervalMs)
                    {
                        statusCallback?.Invoke($"Verifying files, {processedCount} of {totalFiles} ...");
                        lastUpdateTime = currentTime;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return matchingFiles;
        }

        /// <summary>
        /// Asynchronously checks if a given text exists within a file.
        /// </summary>
        /// <param name="filePath">The file to search.</param>
        /// <param name="searchText">The text to find.</param>
        /// <param name="isCaseSensitive">Determines if the search should be case-sensitive.</param>
        /// <param name="ignoreAccents">Determines if accentuation should be ignored.</param>
        /// <returns>True if the text is found, otherwise false.</returns>
        private async Task<bool> ContainsTextInFileAsync(string filePath, string searchText, bool isCaseSensitive, bool ignoreAccents)
        {
            try
            {
                // Set comparison type
                StringComparison comparisonType = isCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

                using StreamReader reader = new StreamReader(filePath);
                List<string> buffer = new List<string>();

                // Read file line by line asynchronously
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        buffer.Add(line); // Defer processing to parallel execution

                        // Process batch when buffer reaches CPU core count
                        if (buffer.Count >= Environment.ProcessorCount)
                        {
                            if (await CheckLinesInParallel(buffer, searchText, comparisonType, ignoreAccents))
                                return true;
                            buffer.Clear();
                        }
                    }
                }

                // Process remaining lines
                return await CheckLinesInParallel(buffer, searchText, comparisonType, ignoreAccents);
            }
            catch (IOException) { /* Ignore file access errors */ }
            catch (UnauthorizedAccessException) { /* Ignore permission errors */ }

            return false;
        }

        ///// <summary>
        ///// Checks if a given text exists within a file.
        ///// </summary>
        ///// <param name="filePath">The file to search.</param>
        ///// <param name="searchText">The text to find.</param>
        ///// <param name="isCaseSensitive">Determines if the search should be case-sensitive.</param>
        ///// <param name="ignoreAccents">Determines if accentuation should be ignored.</param>
        ///// <returns>True if the text is found, otherwise false.</returns>
        //private bool ContainsTextInFile(string filePath, string searchText, bool isCaseSensitive, bool ignoreAccents)
        //{
        //    try
        //    {
        //        StringComparison comparisonType = isCaseSensitive
        //            ? StringComparison.InvariantCulture
        //            : StringComparison.InvariantCultureIgnoreCase;

        //        // Normalize the search text if ignoring accents
        //        if (ignoreAccents)
        //        {
        //            searchText = RemoveDiacritics(searchText);
        //        }

        //        using StreamReader reader = new StreamReader(filePath);

        //        while (!reader.EndOfStream)
        //        {
        //            string line = reader.ReadLine();
        //            if (line != null)
        //            {


        //                string lineToCompare = ignoreAccents ? RemoveDiacritics(line) : line;

        //                if (lineToCompare.Contains(searchText, comparisonType))
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    catch (IOException) { /* Ignore file access errors */ }
        //    catch (UnauthorizedAccessException) { /* Ignore permission errors */ }

        //    return false;
        //}

        /// <summary>
        /// Performs parallel text comparison on a batch of lines.
        /// </summary>
        /// <param name="lines">List of lines to process.</param>
        /// <param name="searchText">Text to search for.</param>
        /// <param name="comparisonType">Case-sensitive or case-insensitive comparison.</param>
        /// <param name="ignoreAccents">Determines if accentuation should be ignored.</param>
        /// <returns>True if a match is found, otherwise false.</returns>
        private Task<bool> CheckLinesInParallel(List<string> lines, string searchText, StringComparison comparisonType, bool ignoreAccents)
        {
            return Task.Run(() =>
            {
                return Parallel.ForEach(lines, (line, state) =>
                {
                    // Convert line inside parallel loop to optimize performance
                    string lineToCompare = ignoreAccents ? RemoveDiacritics(line) : line;

                    if (lineToCompare.Contains(searchText, comparisonType))
                    {
                        state.Break();
                    }
                }).IsCompleted == false;
            });
        }

        /// <summary>
        /// Removes diacritics (accents) from a string to enable accent-insensitive searches.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The normalized string without diacritics.</returns>
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Normalize to FormD (decomposed) and remove non-spacing marks
            try
            {
                return string.Concat(text
    .Normalize(NormalizationForm.FormD)
    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
    .Normalize(NormalizationForm.FormC);  // Normalize back to FormC
            }
            catch (Exception)
            {
                return text;
            }
        }
    }
}
