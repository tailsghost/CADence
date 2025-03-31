using System.IO;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.Abstractions.Clippers;

public static class ClipperExtensions
{
    public static PathsD Render(this PathsD paths, double thickness, bool square, ClipperOffsetD co)
    {
        JoinType joinType = square ? JoinType.Miter : JoinType.Round;
        EndType endType = square ? EndType.Butt : EndType.Round;
        PathsD outPaths = new();

        co.AddPaths(paths, joinType, endType);
        co.Execute(thickness * 0.5, outPaths);
        return outPaths;
    }

}
