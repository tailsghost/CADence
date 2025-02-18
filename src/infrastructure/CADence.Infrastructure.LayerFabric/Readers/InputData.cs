using CADence.Infrastructure.LayerFabric.Common.Abstractions;
using System.Collections.Generic;

namespace CADence.Infrastructure.LayerFabric.Readers;

public class InputData : IInputData
{
    private IDictionary<string, string> _data = new Dictionary<string, string>();

    public IDictionary<string, string> Get()
        => _data;

    public void Set(IDictionary<string, string> data)
        => _data = data;
}