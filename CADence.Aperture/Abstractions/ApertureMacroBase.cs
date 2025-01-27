using CADence.Format;

namespace CADence.Aperture.Abstractions;

internal abstract class ApertureMacroBase
{
    protected List<Expression.Expression> Cmd = [];
    protected List<List<Expression.Expression>> cmds { get; set; } = [];
    public abstract void Append(string cmd);
    public abstract ApertureBase Build(List<string> csep, ApertureFormat fmt);
}
