using CADence.Abstractions.Accuracy;

namespace CADence.Abstractions.Layers;

public interface ICopper
{
    Task<AccuracyBox> GetAccuracy();
}

