using CADence.Abstractions.Apertures;
using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using ExtensionClipper2.Core;

namespace CADence.Core.Apertures.Gerber_274;

public sealed class Circle : ApertureBase
{
    /// <summary>
    /// Диаметр круга.
    /// </summary>
    private double Diameter { get; set; }

    public ApertureBase Render(List<string> csep, ILayerFormat format)
    {
        if (csep.Count < 2 || csep.Count > 3)
        {
            throw new ArgumentException("Invalid circle aperture");
        }

        Diameter = format.ParseFloat(csep[1]);
        HoleDiameter = csep.Count > 2 ? format.ParseFloat(csep[2]) : 0;

        var paths = new PathsD{ new PathD { new PointD(0, 0) }
            }.Render(Diameter, false, format.BuildClipperDrillOffset());

        if (HoleDiameter > 0)
        {
            var hole = GetHole(format);
            paths.AddRange(hole);
        }

        AdditiveGeometry = paths;

        return this;
    }

    /// <summary>
    /// Метод, определяющий, является ли апертура простым кругом (без отверстия).
    /// Если отверстие задано (HoleDiameter > 0), то апертура не является простым кругом.
    /// </summary>
    /// <param name="diameter">Диаметр отверстия.</param>
    /// <returns>True, если апертура является простым кругом (без отверстия); иначе false.</returns>
    public override bool IsSimpleCircle(out double diameter)
    {
        if (HoleDiameter > 0.0)
        {
            diameter = HoleDiameter;
            return false;
        }

        diameter = this.Diameter;
        return true;
    }
}
