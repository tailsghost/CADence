using CADence.Abstractions.Apertures;
using CADence.Abstractions.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Commands.Gerber;

public class AMCommand : ICommand<IGerberSettings>
{
    private IServiceProvider _provider;
    public AMCommand(IServiceProvider provider)
    {
        _provider = provider;
    }
    public IGerberSettings Execute(IGerberSettings settings)
    {
        var name = settings.cmd[2..];
        settings.AmBuilder = _provider.GetRequiredService<IApertureMacro>();
        settings.ApertureMacros[name] = settings.AmBuilder;
        settings.IsDone = true;
        return settings;
    }
}