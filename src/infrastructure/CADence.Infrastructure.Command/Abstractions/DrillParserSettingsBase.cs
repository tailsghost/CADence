using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.Gerber_274x;
using CADence.Infrastructure.Parser.Enums;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Базовый класс параметров для парсера команд дырок.
/// </summary>
public class DrillParserSettingsBase : SettingsBase
{
    /// <summary>
    /// Минимальный размер дырки
    /// </summary>
    public double MinHole { get; set; }

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
    /// Последняя координата.
    /// </summary>
    public Point LastPoint { get; set; }

    /// <summary>
    /// Первая координата.
    /// </summary>
    public Point StartPoint { get; set; }

    /// <summary>
    /// Текущая координата.
    /// </summary>
    public Point Point { get; set; } = new(0, 0);
    
    /// <summary>
    /// Список координат.
    /// </summary>
    
    public List<Point> Points { get; set; } = new(150);

    /// <summary>
    /// Стек апертур.
    /// </summary>
    public Stack<ApertureBase> ApertureStack { get; set; } = [];

    /// <summary>
    /// Формат файла.
    /// </summary>
    public (int integerDigits, int decimalDigits) FileFormat { get; set; }

    /// <summary>
    /// Дырки PLATED
    /// </summary>
    public Drill Pth = new();

    /// <summary>
    /// Дырки NON_PLATED
    /// </summary>
    public Drill Npth = new();
}