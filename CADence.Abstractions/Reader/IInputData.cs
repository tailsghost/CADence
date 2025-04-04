namespace CADence.Abstractions.Readers;

/// <summary>
/// Interface representing input data parsed from an archive.
/// </summary>
public interface IInputData
{
    /// <summary>
    /// Returns a dictionary of file names and their content.
    /// </summary>
    IDictionary<string, string> Get();
}
