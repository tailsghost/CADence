using CADence.Models.Format;

namespace CADence.Infrastructure.Parser.Abstractions;

public abstract class SettingsBase
{
    /// <summary>
    /// Флаг означающий выполнение команды.
    /// </summary>
    public bool IsDone { get; set; }
    
    /// <summary>
    /// Строковое представление команды.
    /// </summary>
    public string cmd { get; set; }

    /// <summary>
    /// H
    /// </summary>
    public LayerFormat format = new();
}