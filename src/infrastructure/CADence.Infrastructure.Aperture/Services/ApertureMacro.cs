using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Models.Format.Abstractions;

namespace CADence.Infrastructure.Aperture.Services;

public class ApertureMacro : ApertureMacroBase
{
    public override void Append(string cmd)
    {
        throw new NotImplementedException();
    }

    public override ApertureBase Build(List<string> csep, LayerFormatBase format)
    {
        throw new NotImplementedException();
    }
}