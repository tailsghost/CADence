using CADence.Aperture;
using CADence.Layer.Enums;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Abstractions;

public abstract class LayerBase(LayerFormatBase format, GerberParserBase parser)
{
    protected GerberParserBase PARSER { get; init; } = parser;

    protected LayerFormatBase Format = format;
    public GerberLayer Layer { get; set; }
    public Color ColorLayer { get; protected set; }
    public double Thickness { get; set; }
    public abstract void Render(); 
    public abstract Geometry GetLayer();
}
