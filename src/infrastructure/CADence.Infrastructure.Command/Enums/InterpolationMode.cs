namespace CADence.Infrastructure.Parser.Enums;

/// <summary>
/// Режим интерполяции, используемый в Gerber файлах.
/// </summary>
public enum InterpolationMode
{
    /// <summary>
    /// Неопределённый режим интерполяции.
    /// </summary>
    UNDEFINED,
    
    /// <summary>
    /// Линейный режим интерполяции.
    /// </summary>
    LINEAR,
    
    /// <summary>
    /// Круговой режим интерполяции по часовой стрелке.
    /// </summary>
    CIRCULAR_CW,
    
    /// <summary>
    /// Круговой режим интерполяции против часовой стрелки.
    /// </summary>
    CIRCULAR_CCW
}