using NetTopologySuite.Geometries;
using System.IO;

namespace CADence.Aperture;

public abstract class ApertureBase
{
    protected double HoleDiametr;
    public List<List<Coordinate>> AccumPaths { get; set; } = new();
    protected bool AccumPolarity { get; set; }
    protected Geometry Dark {  get; set; }
    protected Geometry Clear { get; set; }
    protected bool Simplified { get; set; }
    protected abstract void CommitPaths();

    protected abstract void Simplify();

    public abstract void DrawPaths(List<List<Coordinate>> paths, bool polarity = true);

    public abstract void DrawPath(List<Coordinate> path, bool polarity = true);

    public abstract void DrawPaths(
        List<List<Coordinate>> paths,
        bool polarity,
        double translateX,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0,
        bool specialFillType = false
    );

    public abstract void DrawAperture(
        ApertureBase plot,
        bool polarity = true,
        double translateX = 0,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0
    );

    public abstract Geometry GetDark();

    public abstract Geometry GetClear();

    protected abstract Geometry GetHole();
}
