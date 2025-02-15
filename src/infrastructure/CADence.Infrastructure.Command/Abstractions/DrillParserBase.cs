using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Абстрактный базовый класс для парсера дырок команд.
/// </summary>
public abstract class DrillParserBase
{
    /// <summary>
    /// Результатирующая геометрия дырок
    /// </summary>
    protected Geometry _drillGeometry;

    /// <summary>
    /// Выполняет парсинг команд для дырок.
    /// </summary>
    public abstract void Execute();


    /// <returns>Возвращает результатирующую геометрию дырок</returns>
    public Geometry GetLayer()
    {
        return _drillGeometry;
    }
}