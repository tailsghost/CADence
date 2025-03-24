using CADence.Abstractions.Readers;

namespace CADence.Core.Readers;

public class InputData : IInputData
{
    private IDictionary<string, string> _data = new Dictionary<string, string>();

    public IDictionary<string, string> Get()
        => _data;

    public void Set(IDictionary<string, string> data)
        => _data = data;
}
