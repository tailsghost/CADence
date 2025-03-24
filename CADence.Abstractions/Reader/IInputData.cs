using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADence.Abstractions.Readers;

public interface IInputData
{
    IDictionary<string, string> Get();
}
