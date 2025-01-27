using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using System.IO;

namespace CADence.Aperture;

public abstract class ApertureBase
{
    protected double HoleDiametr;
    public List<List<Coordinate>> AccumPaths { get; set; } = new();
    protected bool AccumPolarity { get; set; }
    protected Geometry Dark { get; set; }
    protected Geometry Clear { get; set; }
    protected bool Simplified { get; set; }
    protected abstract void Simplify();
    public void DrawPaths(List<List<Coordinate>> paths, bool polarity = true)
    {
        if (!paths.Any())
            return;

        if (polarity != AccumPolarity) CommitPaths();
        AccumPolarity = polarity;

        for (int i = 0; i < paths.Count; i++)
        {
            AccumPaths.Add(paths[i]);
        }
    }

    public void DrawPaths(Geometry geometry, bool polarity = true)
    {
        if (geometry == null || !geometry.Coordinates.Any())
            return;

        if (polarity != AccumPolarity) CommitPaths();
        AccumPolarity = polarity;

        List<Coordinate> coordinatesList = new List<Coordinate>();

        for (int i = 0; i < geometry.Coordinates.Length; i++)
        {
            coordinatesList.Add(geometry.Coordinates[i]);
        }

        AccumPaths.Add(coordinatesList);
    }

    public void DrawPath(List<Coordinate> path, bool polarity = true)
    {
        if (!path.Any())
            return;

        if (polarity != AccumPolarity) CommitPaths();
        AccumPolarity = polarity;
        AccumPaths.Add(path);
    }

    protected void CommitPaths()
    {
        if (AccumPaths.Count == 0) return;

        var geometryFactory = new GeometryFactory();
        var pathGeometries = new List<Polygon>();

        for (int i = 0; i < AccumPaths.Count; i++)
        {
            var path = AccumPaths[i];
            var coordinates = new Coordinate[path.Count];
            for (int j = 0; j < path.Count; j++)
            {
                coordinates[j] = new Coordinate(path[j].X, path[j].Y);
            }

            var outerRing = new LinearRing(coordinates);
            var polygon = new Polygon(outerRing);
            pathGeometries.Add(polygon);
        }

        foreach (var polygon in pathGeometries)
        {
            if (AccumPolarity)
            {
                Dark = Dark == null ? polygon : Dark.Union(polygon);
                Clear = Clear == null ? polygon : Clear.Difference(polygon);
            }
            else
            {
                Dark = Dark == null ? polygon : Dark.Difference(polygon);
                Clear = Clear == null ? polygon : Clear.Union(polygon);
            }
        }

        Simplified = false;
        AccumPaths.Clear();
    }

    public void DrawPaths(
    Geometry paths,
    bool polarity,
    double translateX,
    double translateY = 0,
    bool mirrorX = false,
    bool mirrorY = false,
    double rotate = 0.0,
    double scale = 1.0,
    bool specialFillType = false
)
    {
        if (paths.Coordinates.Length == 0) return;

        if (specialFillType) CommitPaths();

        DrawPaths(paths, polarity);

        double ixx = mirrorX ? -scale : scale;
        double iyy = mirrorY ? -scale : scale;
        double sinRot = Math.Sin(rotate);
        double cosRot = Math.Cos(rotate);

        double xx = ixx * cosRot;
        double xy = ixx * sinRot;
        double yx = iyy * -sinRot;
        double yy = iyy * cosRot;

        double cx;
        double cy;

        Coordinate point;

        for (int j = 0; j < paths.NumGeometries; j++)
        {
            var pathCopy = new List<Coordinate>(paths.Coordinates.Length);

            for (int i = 0; i < paths.Coordinates.Length; i++)
            {
                point = paths.Coordinates[i];
                cx = point.X * xx + point.Y * yx;
                cy = point.X * xy + point.Y * yy;

                pathCopy.Add(new Coordinate((Math.Round(cx) + translateX), (Math.Round(cy) + translateY)));
            }

            if (AccumPaths.Count > 0)
            {
                AccumPaths[AccumPaths.Count - 1] = pathCopy;
            }
            else
            {
                AccumPaths.Add(pathCopy);
            }
        }

        if (mirrorX != mirrorY)
        {
            int accumulatedPathCount = AccumPaths.Count;
            int pathsCount = paths.NumGeometries;

            for (int i = accumulatedPathCount - pathsCount; i < accumulatedPathCount; i++)
            {
                AccumPaths[i].Reverse();
            }
        }

        if (specialFillType) CommitPaths();
    }

    public void DrawAperture(
        ApertureBase plot,
        bool polarity = true,
        double translateX = 0,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0
    )
    {
        DrawPaths(plot.Dark, polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
        DrawPaths(plot.Clear, !polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
    }

    public abstract Geometry GetDark();

    public abstract Geometry GetClear();

    protected abstract Geometry GetHole();
}
