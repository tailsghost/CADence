using CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;

/// <summary>
/// ������� ������ ��� Gerber 274x, �������������� ������� �������.
/// </summary>
public class Drill274xFabric : FabricCommandBase<DrillParserSettingsBase>
{
    /// <summary>
    /// ������������ ������� Gerber-�������.
    /// </summary>
    private void InitialCommand()
    {
        Add("AB", () => new ABCommand());
        Add("AD", () => new ADCommand());
        Add("AM", () => new AMCommand());
        Add("FS", () => new FSCommand());
        Add("MO", () => new MOCommand());
        Add("M0", () => new M0Command());
        Add("LP", () => new LPCommand());
        Add("LM", () => new LMCommand());
        Add("D", () => new InstallCommand());
        Add("I", () => new InstallCommand());
        Add("G", () => new GCommand());
        Add("X", () => new InstallCommand());
        Add("Y", () => new InstallCommand());
    }

    /// <summary>
    /// �������������� ����� ��������� <see cref="Gerber274xFabric"/> � ������������ �������.
    /// </summary>
    public Drill274xFabric()
    {
        InitialCommand();
    }
}