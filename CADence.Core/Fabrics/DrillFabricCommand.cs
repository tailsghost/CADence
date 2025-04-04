using CADence.Abstractions.Commands;
using CADence.Core.Commands.Drill;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Fabrics;

internal class DrillFabricCommand : IFabricCommand<IDrillSettings>
{
    private readonly Dictionary<string, Func<ICommand<IDrillSettings>>> _commands = new();

    public DrillFabricCommand(IServiceProvider provider)
    {
        Add("M48", () => provider.GetRequiredService<M48Command>());
        Add("FMAT,2", () => provider.GetRequiredService<NoOpCommand>());
        Add("FILE_FORMAT=", () => provider.GetRequiredService<HeaderCommentCommand>());
        Add("TYPE", () => provider.GetRequiredService<HeaderCommentCommand>());
        Add("VER,1", () => throw new InvalidOperationException("Version 1 excellon is not supported"));
        Add("METRIC", () => provider.GetRequiredService<MetricCommand>());
        Add("INCH", () => provider.GetRequiredService<InchCommand>());
        Add("%", () => provider.GetRequiredService<EndHeaderCommand>());
        Add("M95", () => provider.GetRequiredService<EndHeaderCommand>());
        Add(";", () => provider.GetRequiredService<NoOpCommand>());
        Add("T", () => provider.GetRequiredService<ToolChangeCommand>());
        Add("G", () => provider.GetRequiredService<GCommand>());
        Add("M", () => provider.GetRequiredService<MCommand>());
        Add("X", () => provider.GetRequiredService<CoordinateCommand>());
        Add("Y", () => provider.GetRequiredService<CoordinateCommand>());

    }

    public void Add(string startCommand, Func<ICommand<IDrillSettings>> command)
    {
        if (string.IsNullOrWhiteSpace(startCommand) || command == null)
            throw new ArgumentException("Invalid command parameters.");

        _commands.TryAdd(startCommand, command);
    }

    public IDrillSettings ExecuteCommand(IDrillSettings settings)
    {
        for (int i = 0; i < _commands.Count; i++)
        {
            var item = _commands.ElementAt(i);
            if (settings.cmd.StartsWith(item.Key))
            {
                return item.Value().Execute(settings);
            }
        }

        return settings;
    }

    public void Remove(string startCommand)
        => _commands.Remove(startCommand);
}
