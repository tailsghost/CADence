using CADence.Infrastructure.LayerFabric.Common.Abstractions;

namespace CADence.Infrastructure.LayerFabric.Readers;

public class InputData : IInputData
{
    private Dictionary<string, string> _data = new();

    public IDictionary<string, string> Get()
        => _data;

    public void Set(Dictionary<string, string> data)
        => _data = data;
}