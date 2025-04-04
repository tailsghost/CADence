using CADence.Abstractions.Readers;
using SharpCompress.Archives.Rar;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace CADence.Core.Readers
{
    /// <summary>
    /// Implements IReader for board file parsing.
    /// </summary>
    internal class BoardFileReader : IReader
    {
        /// <summary>
        /// Unsupported file extensions within the archive.
        /// </summary>
        private readonly IEnumerable<string> _unsupported = new List<string>
        {
            "config", "exe", "dll", "png", "zip", "gif", "jpeg", "doc", "docx", "jpg", "bmp", "svg"
        };

        /// <summary>
        /// Parses the archive (ZIP or RAR) and extracts files into a dictionary.
        /// </summary>
        /// <param name="stream">Stream containing the archive.</param>
        /// <param name="fileName">Name of the archive file including extension.</param>
        /// <returns>An instance of IInputData with the parsed file contents.</returns>
        public IInputData ParseArchive(Stream stream, string fileName)
        {
            Dictionary<string, string> data = new();

            if (!fileName.ToLower().EndsWith(".zip") && !fileName.ToLower().EndsWith(".rar"))
            {
                throw new ArgumentException("Unsupported file format. Only ZIP and RAR are supported.");
            }

            if (fileName.ToLower().EndsWith(".zip"))
            {
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    var result = ProcessArchiveEntry(entry.FullName, entry.Name, entry.Open);
                    if (!string.IsNullOrEmpty(result))
                    {
                        data[entry.FullName] = result;
                    }
                }
            }
            else if (fileName.ToLower().EndsWith(".rar"))
            {
                using var archive = RarArchive.Open(stream);
                var entriesArray = archive.Entries.Where(e => !e.IsDirectory).ToArray();

                foreach (var entry in entriesArray)
                {
                    var name = Path.GetFileName(entry.Key);
                    var fullPath = entry.Key;
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(fullPath))
                        continue;
                    var result = ProcessArchiveEntry(fullPath, name, () => entry.OpenEntryStream());
                    if (!string.IsNullOrEmpty(result))
                    {
                        data[fullPath] = result;
                    }
                }
            }

            var input = new InputData();
            input.Set(data);
            return input;
        }

        /// <summary>
        /// Processes an individual file entry in the archive by checking its extension and reading its content.
        /// </summary>
        /// <param name="fullPath">Full path of the file entry in the archive.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="openStream">A function that opens a stream for the file.</param>
        /// <returns>The content of the file as a string, or an empty string if invalid.</returns>
        private string ProcessArchiveEntry(string fullPath, string fileName, Func<Stream> openStream)
        {
            if (!IsValidFile(fileName) || fullPath.EndsWith('/'))
                return string.Empty;

            using var ms = new MemoryStream();
            using (var entryStream = openStream())
            {
                entryStream.CopyTo(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(ms, Encoding.UTF8);
            var result = reader.ReadToEnd();

            return FindFileType(result) ? result : string.Empty;
        }

        /// <summary>
        /// Determines if the file is valid for processing based on its extension.
        /// </summary>
        /// <param name="filename">Name of the file to check.</param>
        /// <returns>True if the file has an allowed extension; otherwise, false.</returns>
        private bool IsValidFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return false;

            var ext = new FileInfo(filename).Extension.ToLower().TrimStart('.');
            return !_unsupported.Contains(ext);
        }

        /// <summary>
        /// Checks if the file content represents a board layer.
        /// </summary>
        /// <param name="file">File content as a string.</param>
        /// <returns>True if the content contains a board layer indicator; otherwise, false.</returns>
        private bool FindFileType(string file)
        {
            return file.Contains("%FS") || file.Contains("M48");
        }
    }
}