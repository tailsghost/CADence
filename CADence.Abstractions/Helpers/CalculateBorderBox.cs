using ExtensionClipper2.Core;

namespace CADence.Abstractions.Helpers;

public static class CalculateBorderBox
{
    public static RectD GetBounds(PathsD border)
    {
        var bounds = new RectD
        {
            Left = double.MaxValue,
            Bottom = double.MaxValue,
            Right = double.MinValue,
            Top = double.MinValue
        };

        foreach (var point in border[0])
        {
            bounds.Left = Math.Min(bounds.Left, point.X);
            bounds.Right = Math.Max(bounds.Right, point.X);
            bounds.Bottom = Math.Min(bounds.Bottom, point.Y);
            bounds.Top = Math.Max(bounds.Top, point.Y);
        }

        return bounds;
    }
}
