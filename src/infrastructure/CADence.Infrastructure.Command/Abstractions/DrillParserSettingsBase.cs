using CADence.Infrastructure.Parser.Enums;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Базовый класс параметров для парсера команд дырок.
/// </summary>
public class DrillParserSettingsBase
{
    /// <summary>
    /// Флаг означающий выполнение команды.
    /// </summary>
    public bool Execute { get; set; }
    
    /// <summary>
    /// Расположение дырки.
    /// </summary>
    public ParseState ParseState { get; set; }
    
    /// <summary>
    /// Режим трассировки.
    /// </summary>
    public RoutMode RoutMode { get; set; }
    
    /// <summary>
    /// Флаг, указывающий, является ли операция платированной.
    /// </summary>
    public bool Plated { get; set; }
    
    /// <summary>
    /// Текущая координата.
    /// </summary>
    public Coordinate Coordinate { get; set; }
}