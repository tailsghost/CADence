using CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;

/// <summary>
/// ������� ������ ��� Drill 274x, �������������� ������� �������.
/// </summary>
public class Drill274xFabric : FabricCommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// ������������ ������� Drill-�������.
    /// </summary>
    private void InitialCommand()
    {
        Add("M48", () => new M48Command());
        Add("FMAT,2", () => new FMAT2Command());
    }

    /// <summary>
    /// �������������� ����� ��������� <see cref="Drill274xFabric"/> � ������������ �������.
    /// </summary>
    public Drill274xFabric()
    {
        InitialCommand();
    }
}