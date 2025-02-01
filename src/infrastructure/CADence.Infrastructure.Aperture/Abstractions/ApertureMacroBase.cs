namespace CADence.Infrastructure.Aperture.Abstractions;

internal abstract class ApertureMacroBase
{
    protected List<CADence.Aperture.Expression.Expression> Cmd = [];
    protected List<List<CADence.Aperture.Expression.Expression>> cmds { get; set; } = [];
    public abstract void Append(string cmd);
    public abstract ApertureBase Build(List<string> csep, ApertureBase format);
}
