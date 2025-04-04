using CADence.Abstractions.Commands;
using CADence.Core.Commands.Gerber;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Fabrics;

internal class GerberFabricCommand : IFabricCommand<IGerberSettings>
{
    private readonly Dictionary<string, Func<ICommand<IGerberSettings>>> _commands = new();

    public GerberFabricCommand(IServiceProvider provider)
    {
        Add("AB", () => provider.GetRequiredService<ABCommand>());
        Add("AD", () => provider.GetRequiredService<ADCommand>());
        Add("AM", () => provider.GetRequiredService<AMCommand>());
        Add("FS", () => provider.GetRequiredService<FSCommand>());
        Add("MO", () => provider.GetRequiredService<MOCommand>());
        Add("M0", () => provider.GetRequiredService<M0Command>());
        Add("L", () => provider.GetRequiredService<LCommand>());
        Add("D", () => provider.GetRequiredService<InstallCommand>());
        Add("I", () => provider.GetRequiredService<InstallCommand>());
        Add("G", () => provider.GetRequiredService<GCommand>());
        Add("X", () => provider.GetRequiredService<InstallCommand>());
        Add("Y", () => provider.GetRequiredService<InstallCommand>());

    }

    public void Add(string startCommand, Func<ICommand<IGerberSettings>> command)
    {
        if (string.IsNullOrWhiteSpace(startCommand) || command == null)
            throw new ArgumentException("Invalid command parameters.");

        _commands.TryAdd(startCommand, command);
    }

    public IGerberSettings ExecuteCommand(IGerberSettings settings)
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

