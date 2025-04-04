using CADence.Abstractions.Apertures;
using CADence.Abstractions.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Commands.Gerber;

/// <summary>
/// Command to start an aperture macro definition (AM command) in Gerber files.
/// </summary>
internal class AMCommand : ICommand<IGerberSettings>
{
    private IServiceProvider _provider;
    public AMCommand(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Executes the AM command by creating a new aperture macro builder.
    /// </summary>
    /// <param name="settings">The current Gerber settings.</param>
    /// <returns>The updated Gerber settings.</returns>
    public IGerberSettings Execute(IGerberSettings settings)
    {
        var name = settings.cmd[2..];
        settings.AmBuilder = _provider.GetRequiredService<IApertureMacro>();
        settings.ApertureMacros[name] = settings.AmBuilder;
        settings.IsDone = true;
        return settings;
    }
}