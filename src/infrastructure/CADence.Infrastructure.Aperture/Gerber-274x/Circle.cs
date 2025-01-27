using CADence.Aperture.Abstractions;
using NetTopologySuite.Geometries;

namespace CADence.Aperture.Gerber_274x;

public class Circle : ApertureBase
{
    public override void DrawAperture(ApertureBase plot, bool polarity = true, double translateX = 0, double translateY = 0, bool mirrorX = false, bool mirrorY = false, double rotate = 0, double scale = 1)
    {
        throw new NotImplementedException();
    }

    public override void DrawPath(List<Coordinate> path, bool polarity = true)
    {
        throw new NotImplementedException();
    }

    public override void DrawPaths(List<List<Coordinate>> paths, bool polarity = true)
    {
        throw new NotImplementedException();
    }

    public override void DrawPaths(List<List<Coordinate>> paths, bool polarity, double translateX, double translateY = 0, bool mirrorX = false, bool mirrorY = false, double rotate = 0, double scale = 1, bool specialFillType = false)
    {
        throw new NotImplementedException();
    }

    public override Geometry GetClear()
    {
        throw new NotImplementedException();
    }

    public override Geometry GetDark()
    {
        throw new NotImplementedException();
    }

    protected override void CommitPaths()
    {
        throw new NotImplementedException();
    }

    protected override Geometry GetHole()
    {
        throw new NotImplementedException();
    }

    protected override void Simplify()
    {
        throw new NotImplementedException();
    }
}
