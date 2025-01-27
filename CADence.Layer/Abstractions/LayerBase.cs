using CADence.Aperture;
using CADence.Format.Abstractions;
using CADence.Layer.Enums;
using NetTopologySuite.Geometries;
using System.Text;

namespace CADence.Layer.Abstractions;

public abstract class LayerBase
{

    protected LayerBase(FormatBase format)
    {
        Format = format;
    }

    protected FormatBase Format;
    public GerberLayer Layer { get; set; }
    public double Thickness { get; set; }
    public abstract void Render();
    public abstract StringBuilder ToSvg(Geometry geometry, Color.Color? color, StringBuilder Data);
    public abstract StringBuilder ToJson(Geometry geometry, Color.Color? color, StringBuilder Data);
    public abstract StringBuilder ToSvg(List<Geometry> geometry, Color.Color? color, StringBuilder Data);
    public abstract StringBuilder ToJson(List<Geometry> geometry, Color.Color? color, StringBuilder Data);
}
