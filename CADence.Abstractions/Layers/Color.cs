namespace CADence.App.Abstractions.Layers;

public struct Color
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }
}

public class ColorConstants
{
    public static Color NONE { get; } = new Color { R = 0.0f, G = 0.0f, B = 0.0f, A = 0.0f };

    public static Color BLACK { get; } = new Color { R = 0.0f, G = 0.0f, B = 0.0f, A = 1.0f };

    public static Color COPPER { get; } = new Color { R = 1.0f, G = 0.7f, B = 0.3f, A = 1.0f };

    public static Color FINISH_TIN { get; } = new Color { R = 0.7f, G = 0.7f, B = 0.7f, A = 1.0f };

    public static Color SUBSTRATE { get; } = new Color { R = 0.95f, G = 0.5f, B = 0.3f, A = 0.98f };

    public static Color MASK_GREEN { get; } = new Color { R = 0.1f, G = 0.6f, B = 0.3f, A = 0.6f };

    public static Color MASK_WHITE { get; } = new Color { R = 0.9f, G = 0.93f, B = 1.0f, A = 0.8f };

    public static Color SILK_WHITE { get; } = new Color { R = 0.9f, G = 0.9f, B = 0.9f, A = 0.9f };

    public static Color SILK_BLACK { get; } = new Color { R = 0.1f, G = 0.1f, B = 0.1f, A = 0.9f };
}
