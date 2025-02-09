using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Абстрактный базовый класс для парсера дырок команд.
/// </summary>
public abstract class DrillParserBase
{
    /// <summary>
    /// Выполняет парсинг команд для дырок.
    /// </summary>
    public abstract void Execute();


    /// <summary>
    /// Выполняет рендеринг дырок
    /// </summary>
    /// <returns>Возвращает геометрию дырок</returns>
    public abstract Geometry GetResult(bool plated = true, bool unplated = true);
}