using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;
using System.Runtime.Intrinsics.X86;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;

/// <summary>
/// Команда "Install" для Gerber-парсера.
/// </summary>
public class InstallCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "Install" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        var parameters = new Dictionary<char, double> { { 'X', settings.Pos.X }, { 'Y', settings.Pos.Y }, { 'I', 0 }, { 'J', 0 } };

        var d = -1;
        var code = ' ';
        var start = 0;

        for (int i = 0; i <= cmd.Length; i++)
        {
            char c = (i < cmd.Length) ? cmd[i] : 'Z';
            if (i == cmd.Length || char.IsLetter(c))
            {
                if (code == 'D')
                    d = int.Parse(cmd[start..i]);
                else if (code == 'I' || code == 'J')
                {
                    parameters[code] = fmt.ParseFixed(cmd[start..i]);
                }
                else if (code != ' ')
                    parameters[code] = fmt.ParseFixed(cmd[start..i]);

                code = c;
                start = i + 1;
            }
        }
    }
}