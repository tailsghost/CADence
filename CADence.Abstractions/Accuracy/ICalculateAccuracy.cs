using ExtensionClipper2.Core;

namespace CADence.Abstractions.Accuracy;

public interface ICalculateAccuracy
{
    AccuracyBox StartCalculate(PathsD copper, double radius);
}

