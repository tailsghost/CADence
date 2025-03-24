namespace CADence.Abstractions.Commands;

/// <summary>
/// Режим работы с квадрантами в Gerber файлах.
/// </summary>
public enum QuadrantMode
{
    /// <summary>
    /// Неопределённый режим.
    /// </summary>
    UNDEFINED,
    
    /// <summary>
    /// Одинарный режим.
    /// </summary>
    SINGLE,
    
    /// <summary>
    /// Множественный режим.
    /// </summary>
    MULTI
}