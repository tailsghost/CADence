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

    private Gerber274xFabric _fabric = new();

    /// <summary>
    /// Инициализирует новый экземпляр парсера Gerber 274X.
    /// </summary>
    /// <param name="file">Строковое содержимое Gerber файла.</param>
    public GerberParser274X(string file)
    {
        FILE = file;
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

        while (stream.Peek() != -1)
        {
            var c = (char)stream.Read();
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

                var cmd = ss.ToString();

                _settings.AmBuilder?.Append(cmd);

                _settings.cmd = cmd;

                _settings = _fabric.ExecuteCommand(_settings);

                if (!_settings.IsDone)
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
}