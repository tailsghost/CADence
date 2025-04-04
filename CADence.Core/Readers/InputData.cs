using CADence.Abstractions.Readers;

namespace CADence.Core.Readers;

/// <summary>
/// Implements IInputData to store and retrieve parsed file data.
/// </summary>
internal class InputData : IInputData
{
    private IDictionary<string, string> _data = new Dictionary<string, string>();

    /// <summary>
    /// Gets the stored file data.
    /// </summary>
    public IDictionary<string, string> Get()
        => _data;


    /// <summary>
    /// Sets the file data.
    /// </summary>
    /// <param name="data">The dictionary containing file data.</param>
    public void Set(IDictionary<string, string> data)
        => _data = data;
}
