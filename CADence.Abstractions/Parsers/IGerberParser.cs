using ExtensionClipper2.Core;

namespace CADence.App.Abstractions.Parsers;

/// <summary>
/// Interface for parsing Gerber files.
/// </summary>
public interface IGerberParser
{
    /// <summary>
    /// Executes the Gerber parsing for the given file content.
    /// </summary>
    IGerberParser Execute(string file);

    /// <summary>
    /// Gets the resulting Gerber geometry.
    /// </summary>
    /// <param name="BoardOutLine">Flag to indicate if board outline should be processed.</param>
    PathsD GetResult(bool BoardOutLine = false);

    /// <summary>
    /// Gets the minimum thickness from the parsed Gerber file.
    /// </summary>
    double GetMinimumThickness();
}
