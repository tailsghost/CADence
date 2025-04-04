using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADence.Abstractions.Readers;

/// <summary>
/// Interface for reading archives (ZIP or RAR) and extracting file content.
/// </summary>
public interface IReader
{
    /// <summary>
    /// Parses an archive and extracts files, returning their content in a dictionary.
    /// </summary>
    /// <param name="stream">The data stream containing the archive.</param>
    /// <param name="fileName">The name of the archive including its extension.</param>
    /// <returns>An instance of IInputData with the extracted content.</returns>
    IInputData ParseArchive(Stream stream, string fileName);
}
