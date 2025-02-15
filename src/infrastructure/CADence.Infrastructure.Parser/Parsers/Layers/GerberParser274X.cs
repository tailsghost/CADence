using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;
using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;

namespace CADence.Infrastructure.Parser.Parsers;

/// <summary>
/// Парсер Gerber файлов формата 274X.
/// </summary>
public class GerberParser274X : GerberParserBase
{
    private GeometryFactory _geometryFactory;

    /// <summary>
    /// Содержит содержимое Gerber файла.
    /// </summary>
    public string FILE = string.Empty;

    /// <summary>
    /// Параметры для парсинга Gerber файлов формата 274X.
    /// </summary>
    private GerberParser274xSettings _settings = new();

    /// <summary>
    /// Фабрика комманд
    /// </summary>
    private Gerber274xFabric _fabric = new();

    /// <summary>
    /// Логирование
    /// </summary>
    private readonly NLog.ILogger _logger;

    /// <summary>
    /// Инициализирует новый экземпляр парсера Gerber 274X.
    /// </summary>
    /// <param name="file">Строковое содержимое Gerber файла.</param>
    public GerberParser274X(string file)
    {
        _logger = NLog.LogManager.GetCurrentClassLogger();
        FILE = file;
        _geometryFactory = new();
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
        try
        {

            using var stream = new StringReader(FILE);

            SetupSettings();

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

            if (_settings.IsDone)
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при выполнении парсинга.");
            throw new InvalidOperationException("Ошибка при выполнении парсинга.", ex);
        }
    }

    /// <summary>
    /// Рендеринг слоя
    /// </summary>
    /// <param name="BoardOutLine"><c>true</c>, если слой для SUBSTRATE, <c>false</c> для других слоев</param>
    /// <returns></returns>
    public override Geometry GetResult(bool BoardOutLine = false)
    {

        if (BoardOutLine)
        {
            return FindLargestAreaPathWithoutBorderBox(_settings.ApertureStack.Peek().GetAdditive());
        }

        return _settings.ApertureStack.Peek().GetAdditive();
    }


    /// <summary>
    /// Достает из геометрии BorderBox и вырезает из него остальные полигоны
    /// </summary>
    /// <param name="multiPolygon">Геометрия SUBSTRATE слоя</param>
    /// <returns></returns>
    private Geometry FindLargestAreaPathWithoutBorderBox(Geometry multiPolygon)
    {
        if (multiPolygon == null || multiPolygon.IsEmpty)
        {
            return multiPolygon;
        }

        double maxArea = double.MinValue;
        Polygon largestPolygon = null;

        var envelope = multiPolygon.EnvelopeInternal;

        Coordinate[] borderBoxCoords = new Coordinate[]
        {
            new Coordinate(envelope.MinX, envelope.MinY),
            new Coordinate(envelope.MaxX, envelope.MinY),
            new Coordinate(envelope.MaxX, envelope.MaxY),
            new Coordinate(envelope.MinX, envelope.MaxY),
            new Coordinate(envelope.MinX, envelope.MinY)
        };

        Polygon borderBox = _geometryFactory.CreatePolygon(borderBoxCoords);

        Geometry borderBoxWithHoles = borderBox;
        for (int i = 0; i < multiPolygon.NumGeometries; i++)
        {
            Geometry subGeometry = multiPolygon.GetGeometryN(i);
            borderBoxWithHoles = borderBoxWithHoles.Difference(subGeometry);
        }

        return borderBoxWithHoles;

    }

    /// <summary>
    /// Начальная настройка параметров
    /// </summary>
    private void SetupSettings()
    {
        _settings.imode = InterpolationMode.UNDEFINED;
        _settings.qmode = QuadrantMode.UNDEFINED;
        _settings.Pos = new Point(0, 0);
        _settings.Polarity = true;
        _settings.apMirrorX = false;
        _settings.apMirrorY = false;
        _settings.apRotate = 0.0;
        _settings.apScale = 1.0;
        _settings.ApertureStack = new Stack<Aperture.Abstractions.ApertureBase>();
        _settings.ApertureStack.Push(new Aperture.Abstractions.ApertureBase());
        _settings.RegionMode = false;
        _settings.OutlineConstructed = false;
    }

}
