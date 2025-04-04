using CADence.Abstractions.Apertures;
using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using ExtensionClipper2.Core;

namespace CADence.Core.Apertures.Gerber_274;

/// <summary>
/// Represents a circle aperture.
/// </summary>
internal sealed class Circle : ApertureBase
{
    /// <summary>
    /// The diameter of the circle.
    /// </summary>
    private double Diameter { get; set; }

    /// <summary>
    /// Renders the circle aperture based on the given parameters.
    /// </summary>
    /// <param name="csep">List of parameters as strings.</param>
    /// <param name="format">Layer format instance for parsing.</param>
    /// <returns>The rendered aperture.</returns>
    public ApertureBase Render(List<string> csep, ILayerFormat format)
    {
        if (csep.Count is < 2 or > 3)
        {
            throw new ArgumentException("Invalid circle aperture");
        }

        Diameter = format.ParseFloat(csep[1]);
        HoleDiameter = csep.Count > 2 ? format.ParseFloat(csep[2]) : 0;

        var paths = new PathsD{ new PathD { new PointD(0, 0) }
            }.Render(Diameter, false, format.BuildClipperOffset());

        if (HoleDiameter > 0)
        {
            var hole = GetHole(format);
            paths.AddRange(hole);
        }

        AdditiveGeometry = paths;

        return this;
    }

    /// <summary>
    /// Determines whether the aperture is a simple circle (without a hole).
    /// If a hole is defined (HoleDiameter > 0), then the aperture is not simple.
    /// </summary>
    /// <param name="diameter">Outputs the diameter value.</param>
    /// <returns>True if it is a simple circle; otherwise, false.</returns>
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
