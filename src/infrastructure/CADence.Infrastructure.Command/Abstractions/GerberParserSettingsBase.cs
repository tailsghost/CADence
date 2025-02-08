using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Models.Format;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Базовый класс параметров для парсера Gerber.
/// Содержит параметры интерполяции, позиционирования и режимов обработки.
/// </summary>
public class GerberParserSettingsBase : SettingsBase
{
    /// <summary>
    /// Режим интерполяции.
    /// </summary>
    public InterpolationMode imode { get; set; }
    
    /// <summary>
    /// Режим работы с квадрантами.
    /// </summary>
    public QuadrantMode qmode { get; set; }
    
    /// <summary>
    /// Текущая позиция.
    /// </summary>
    public Coordinate Pos { get; set; }
    
    /// <summary>
    /// Полярность текущей операции.
    /// </summary>
    public bool Polarity { get; set; }
    
    /// <summary>
    /// Флаг, указывающий на зеркальное отражение апертуры по оси X.
    /// </summary>
    public bool apMirrorX { get; set; }
    
    /// <summary>
    /// Флаг, указывающий на зеркальное отражение апертуры по оси Y.
    /// </summary>
    public bool apMirrorY { get; set; }
    
    /// <summary>
    /// Угол поворота апертуры в радианах.
    /// </summary>
    public double apRotate { get; set; }
    
    /// <summary>
    /// Масштаб апертуры.
    /// </summary>
    public double apScale { get; set; }
    
    /// <summary>
    /// Стек апертур.
    /// </summary>
    public Stack<ApertureBase> ApertureStack { get; set; } = [];
    
    /// <summary>
    /// Словарь макросов апертуры, сопоставленных с их идентификаторами.
    /// </summary>
    public Dictionary<string, ApertureMacroBase> ApertureMacros = [];
    
    /// <summary>
    /// Режим региона.
    /// </summary>
    public bool RegionMode { get; set; }
    
    /// <summary>
    /// Флаг, указывающий, что контур был построен.
    /// </summary>
    public bool OutlineConstructed { get; set; }
}