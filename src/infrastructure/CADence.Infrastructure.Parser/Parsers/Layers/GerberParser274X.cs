using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Parsers;

/// <summary>
/// Парсер Gerber файлов формата 274X.
/// </summary>
public class GerberParser274X : GerberParserBase
{
    /// <summary>
    /// Содержит содержимое Gerber файла.
    /// </summary>
    public string FILE = string.Empty;

    /// <summary>
    /// Параметры для парсинга Gerber файлов формата 274X.
    /// </summary>
    private GerberParser274xSettings _settings = new();

    private Gerber274xFabric _fabric;

    /// <summary>
    /// Инициализирует новый экземпляр парсера Gerber 274X.
    /// </summary>
    /// <param name="file">Строковое содержимое Gerber файла.</param>
    public GerberParser274X(string file)
    {
        FILE = file;
        _fabric = new Gerber274xFabric();
        Execute();
    }

    /// <summary>
    /// Выполняет парсинг Gerber файла.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если в файле обнаружены ошибки формата.
    /// </exception>
    public override void Execute()
    {
        using var stream = new StringReader(FILE);
        var terminated = false;
        var is_attrib = false;
        var ss = new StringBuilder();
        char c;

        while (stream.Peek() != -1)
        {
            c = (char)stream.Read();
            if (char.IsWhiteSpace(c))
            {
                continue;
            }
            else if (c == '%')
            {
                if (ss.Length > 0) throw new InvalidOperationException("attribute mid-command");
                if (is_attrib) _settings.AmBuilder = null;
                is_attrib = !is_attrib;
            }
            else if (c == '*')
            {
                if (ss.Length == 0) throw new InvalidOperationException("empty command");
                if (!ExecuteCommand(ss.ToString()))
                {
                    terminated = true;
                    break;
                }

                ss.Clear();
            }
            else
            {
                ss.Append(c);
            }
        }

        if (is_attrib)
        {
            throw new InvalidOperationException("unterminated attribute");
        }

        if (!terminated)
        {
            throw new InvalidOperationException("unterminated gerber file");
        }

        if (_settings.ApertureStack.Count != 1)
        {
            throw new InvalidOperationException("unterminated block aperture");
        }

        if (_settings.RegionMode)
        {
            throw new InvalidOperationException("unterminated region block");
        }
    }


    /// <summary>
    /// Выполняет команду Gerber файла.
    /// </summary>
    /// <param name="cmd">Строковое представление команды.</param>
    /// <returns>
    /// True, если команда успешно обработана; иначе false.
    /// </returns>
    private bool ExecuteCommand(string cmd)
    {
        if (_settings.AmBuilder != null)
        {
            _settings.AmBuilder.Append(cmd);
            return true;
        }

        _settings = _fabric.ExecuteCommand(cmd, _settings);

        return _settings.Execute;
    }
}