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
