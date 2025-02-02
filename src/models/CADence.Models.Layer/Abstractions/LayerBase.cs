using CADence.Aperture;
using CADence.Layer.Enums;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;

namespace CADence.Layer.Abstractions;

public abstract class LayerBase(ApertureFormatBase format, IParser parser)
{
    private IParser PARSER { get; init; } = parser;

    protected ApertureFormatBase Format = format;
    public GerberLayer Layer { get; set; }
    public double Thickness { get; set; }
    public abstract void Render();
    public abstract StringBuilder ToSvg(Geometry geometry, Color.Color? color, StringBuilder data);
    public abstract StringBuilder ToJson(Geometry geometry, Color.Color? color, StringBuilder data);
    public abstract StringBuilder ToSvg(List<Geometry> geometry, Color.Color? color, StringBuilder data);
    public abstract StringBuilder ToJson(List<Geometry> geometry, Color.Color? color, StringBuilder data);
}
