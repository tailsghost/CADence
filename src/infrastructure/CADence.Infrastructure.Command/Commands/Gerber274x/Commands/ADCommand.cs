using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;


/// <summary>
/// Команда "A" для Gerber-парсера.
/// </summary>
public class ADCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "AD" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        return settings;
    }
}