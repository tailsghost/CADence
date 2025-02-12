using CADence.Layer.Colors;

namespace CADence.Models.Layer.Colors;

public class ColorScheme
{
    public Color soldermask { get; }

    public Color silkscreen { get; }

    public Color finish { get; }

    public Color substrate { get; }

    public Color copper { get; }

    public ColorScheme()
    {
        soldermask = ColorConstants.MASK_GREEN;
        silkscreen = ColorConstants.SILK_WHITE;
        finish = ColorConstants.FINISH_TIN;
        substrate = ColorConstants.SUBSTRATE;
        copper = ColorConstants.COPPER;
    }

    public ColorScheme(Color soldermask, Color silkscreen,
        Color finish, Color substrate, Color copper)
    {
        this.soldermask = soldermask;
        this.silkscreen = silkscreen;
        this.finish = finish;
        this.substrate = substrate;
        this.copper = copper;
    }
}
