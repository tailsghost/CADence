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
    /// Словарь макросов апертуры, сопоставленных с их идентификаторами.
    /// </summary>
    public Dictionary<string, ApertureMacroBase> ApertureMacros = [];
    
    /// <summary>
    /// Текущий макрос апертуры.
    /// </summary>
    public ApertureMacroBase AmBuilder;
}