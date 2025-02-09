namespace CADence.Infrastructure.Command.Property.Gerber274x;

/// <summary>
/// Инструмент
/// </summary>
public class Tool
{
    /// <summary>
    /// Диаметр инструмента
    /// </summary>
    public double diameter { get; }
    
    /// <summary>
    /// Тип инструмента
    /// </summary>
    public bool plated { get; }

    /// <summary>
    /// Инициализирует инструмент
    /// </summary>
    /// <param name="diameter">Диаметр</param>
    /// <param name="plated">Тип</param>
    public Tool(double diameter, bool plated)
    {
        this.diameter = diameter;
        this.plated = plated;
    }
}