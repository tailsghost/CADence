using System.Collections.Generic;

namespace CADence.Infrastructure.LayerFabric.Common.Abstractions
{
    public interface IInputData
    {
        IDictionary<string, string> Get();
    }
}
