using CADence.Abstractions.Apertures;

namespace CADence.Core.Apertures.Gerber_274;

/// <summary>
/// Инструмент
/// </summary>
internal class Tool : ITool
{
    public double diameter { get; set; }

    public bool plated { get; set; }

    public ITool Init(double diameter, bool plated)
    {
        this.diameter = diameter;
        this.plated = plated;
        return this;
    }
}