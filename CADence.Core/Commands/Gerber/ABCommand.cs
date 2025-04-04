using CADence.Abstractions.Commands;
using CADence.Core.Apertures.Gerber_274;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Commands.Gerber;

/// <summary>
/// Command to handle aperture block closing and opening in Gerber files.
/// </summary>
internal class ABCommand : ICommand<IGerberSettings>
{
    private IServiceProvider _provider;
    public ABCommand(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Executes the AB command to close or open an aperture block.
    /// </summary>
    /// <param name="settings">The current Gerber settings.</param>
    /// <returns>The updated Gerber settings.</returns>
    public IGerberSettings Execute(IGerberSettings settings)
    {
        if (settings.cmd == "AB")
        {
            if (settings.ApertureStack.Count <= 1)
                throw new Exception("Unmatched aperture block close command");
            settings.ApertureStack.Pop();
        }
        else
        {
            var index = int.Parse(settings.cmd[3..]);
            if (index < 10)
                throw new Exception("Aperture index out of range: " + settings.cmd);

            var unknown = _provider.GetRequiredService<Unknown>();
            settings.ApertureStack.Push(unknown);
            settings.Apertures[index] = unknown;
        }

        settings.IsDone = true;

        return settings;
    }
}