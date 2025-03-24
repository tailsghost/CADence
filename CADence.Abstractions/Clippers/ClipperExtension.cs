using Clipper2Lib;
using System.IO;

namespace CADence.Abstractions.Clippers;

public static class ClipperExtensions
{
    public static PathsD Render(this PathsD paths, double thickness, bool square, ClipperOffsetD co)
    {
        JoinType joinType = square ? JoinType.Miter : JoinType.Round;
        EndType endType = square ? EndType.Butt : EndType.Round;

        co.AddPaths(paths, joinType, endType);

        PathsD outPaths = new();

        co.Execute(thickness * 0.5, outPaths);

        return outPaths;
    }
}
