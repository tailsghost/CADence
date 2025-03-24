namespace CADence.Abstractions.Apertures;

public interface ITool
{
    double diameter { get; }

    bool plated { get; }

    public ITool Init(double diameter, bool plated);
}
