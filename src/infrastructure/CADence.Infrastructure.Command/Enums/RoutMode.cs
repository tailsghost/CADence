namespace CADence.Infrastructure.Parser.Enums;

/// <summary>
/// Режим маршрутизации для сверлильных операций.
/// </summary>
public enum RoutMode
{
    /// <summary>
    /// Сверление.
    /// </summary>
    DRILL,
    
    /// <summary>
    /// Маршрутизация с поднятым инструментом.
    /// </summary>
    ROUT_TOOL_UP,
    
    /// <summary>
    /// Маршрутизация с опущенным инструментом.
    /// </summary>
    ROUT_TOOL_DOWN
}