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
}