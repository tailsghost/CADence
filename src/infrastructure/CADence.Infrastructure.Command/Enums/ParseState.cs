namespace CADence.Infrastructure.Parser.Enums;

/// <summary>
/// Состояние парсинга Gerber файла.
/// </summary>
public enum ParseState
{
    /// <summary>
    /// Предварительный заголовок.
    /// </summary>
    PRE_HEADER,
    
    /// <summary>
    /// Заголовок.
    /// </summary>
    HEADER,
    
    /// <summary>
    /// Тело файла.
    /// </summary>
    BODY
}