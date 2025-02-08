using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Базовый класс параметров для парсера команд дырок.
/// </summary>
public class DrillParserSettingsBase : SettingsBase
{
    
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

    /// <summary>
    /// Стек апертур.
    /// </summary>
    public Stack<ApertureBase> ApertureStack { get; set; } = [];

    /// <summary>
    /// Формат файла.
    /// </summary>
    public (int integerDigits, int decimalDigits) FileFormat { get; set; }
}