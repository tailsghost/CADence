using System.IO;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.Abstractions.Clippers;

/// <summary>
/// Extension methods for the Clipper library.
/// </summary>
public static class ClipperExtensions
{
    /// <summary>
    /// Renders the specified paths with a given thickness and join style using a Clipper offset.
    /// </summary>
    /// <param name="paths">The input paths to render.</param>
    /// <param name="thickness">The thickness used for rendering.</param>
    /// <param name="square">If set to <c>true</c>, square (miter) joins are used; otherwise, round joins are used.</param>
    /// <param name="co">An instance of <see cref="ClipperOffsetD"/> used to perform the offset.</param>
    /// <returns>A new set of paths representing the rendered geometry.</returns>
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
