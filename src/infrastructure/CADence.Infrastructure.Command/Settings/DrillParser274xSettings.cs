using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Parser.Abstractions;

namespace CADence.Infrastructure.Parser.Settings;

/// <summary>
/// Настройки для парсера Drill 274X.
/// Наследует базовые параметры Drill-парсера.
/// </summary>
public class DrillParser274xSettings : DrillParserSettingsBase
{
    /// <summary>
    /// Текущая аперткура
    /// </summary>
    public ApertureBase Aperture;

    /// <summary>
    /// Словарь апертур
    /// </summary>
    public Dictionary<int, ApertureBase> Apertures = [];

    /// <summary>
    /// Текущий макрос апертуры.
    /// </summary>
    public ApertureMacroBase AmBuilder;
}