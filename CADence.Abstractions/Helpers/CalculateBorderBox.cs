using ExtensionClipper2.Core;

namespace CADence.Abstractions.Helpers;

/// <summary>
/// Helper class for calculating the bounding rectangle for a given border defined by multiple paths.
/// </summary>
public static class CalculateBorderBox
{
    /// <summary>
    /// Computes the bounding rectangle of a border represented as a list of paths.
    /// </summary>
    /// <param name="border">A collection of paths representing the border.</param>
    /// <returns>A RectD structure that defines the bounds of the border.</returns>
    public static RectD GetBounds(PathsD border)
    {
        var bounds = new RectD
        {
            Left = double.MaxValue,
            Bottom = double.MaxValue,
            Right = double.MinValue,
            Top = double.MinValue
        };

        for (var i = 0; i < border.Count; i++)
        {
            for (var j = 0; j < border[i].Count; j++)
            {
                var point = border[i][j];
                bounds.Left = Math.Min(bounds.Left, point.X);
                bounds.Right = Math.Max(bounds.Right, point.X);
                bounds.Bottom = Math.Min(bounds.Bottom, point.Y);
                bounds.Top = Math.Max(bounds.Top, point.Y);
            }
        }

        return bounds;
    }
}
