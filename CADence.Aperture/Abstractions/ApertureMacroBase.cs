namespace CADence.Aperture.Abstractions;

internal abstract class ApertureMacroBase
{
    protected List<Expression.Expression> Cmd = [];
    protected List<List<Expression.Expression>> cmds { get; set; } = [];
    public abstract void Append(string cmd);
    public abstract Base Build(List<string> csep, Format fmt);
}
