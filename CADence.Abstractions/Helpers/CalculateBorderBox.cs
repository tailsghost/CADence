using Clipper2Lib;

namespace CADence.Abstractions.Helpers;

public static class CalculateBorderBox
{
    public static RectD GetBounds(PathsD border)
    {
        var bounds = new RectD
        {
            left = double.MaxValue,
            bottom = double.MaxValue,
            right = double.MinValue,
            top = double.MinValue
        };

        foreach (var point in border[0])
        {
            bounds.left = Math.Min(bounds.left, point.x);
            bounds.right = Math.Max(bounds.right, point.x);
            bounds.bottom = Math.Min(bounds.bottom, point.y);
            bounds.top = Math.Max(bounds.top, point.y);
        }

        return bounds;
    }
}
