using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;

public class M0Command : CommandBase<GerberParser274xSettings>
{
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        if (settings.cmd == "M00" || settings.cmd == "M01" || settings.cmd == "M02")
            settings.IsDone = false;

        return settings;
    }
}