using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Aperture.Abstractions;
namespace CADence.Infrastructure.Parser.Settings;

/// <summary>
/// Настройки для парсера Gerber 274X.
/// Наследует базовые параметры Gerber-парсера.
/// </summary>
public class GerberParser274xSettings : GerberParserSettingsBase
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