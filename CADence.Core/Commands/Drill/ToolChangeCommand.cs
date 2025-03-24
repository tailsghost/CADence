using CADence.Abstractions.Apertures;
using CADence.Abstractions.Commands;
using CADence.Core.Apertures.Gerber_274;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Commands.Drill;

public class ToolChangeCommand : ICommand<IDrillSettings>
{
    private readonly IServiceProvider _provider;
    public ToolChangeCommand(IServiceProvider provider)
    {
        _provider = provider;
    }

    public  IDrillSettings Execute(IDrillSettings settings)
    {
        if (settings.ParseState == ParseState.BODY)
        {
            var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
            if (!dict.TryGetValue('T', out var tStr)) return settings;

            var t = int.Parse(tStr);
            if (settings.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                throw new InvalidOperationException("unexpected tool change; tool is down");

            if (t == 0)
            {
                settings.Tool = null;
                settings.IsDone = true;
                return settings;
            }

            if (!settings.Tools.TryGetValue(t, out var tool))
                throw new InvalidOperationException("attempting to change to undefined tool: " + t);

            settings.Tool = tool;
            settings.IsDone = true;
            return settings;
        }
        else if (settings.ParseState == ParseState.HEADER)
        {
            var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
            if (!dict.TryGetValue('T', out var toolStr)) return settings;
        
            var toolNo = int.Parse(toolStr);
            if (!dict.TryGetValue('C', out var diameter))
                throw new InvalidOperationException("missing tool diameter in " + settings.fcmd);

            ITool Tool = _provider.GetRequiredService<Tool>();

            settings.Tools[toolNo] = Tool.Init(settings.format.ParseFloat(diameter), settings.Plated);

            settings.IsDone = true;
            return settings;
        }

        return settings;
    }
}