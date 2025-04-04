using CADence.Abstractions.Clippers;
using ExtensionClipper2;

namespace CADence.App.Abstractions.Formats;

public interface ILayerFormat
{
    void ConfigureFormat(int integerDigits, int decimalDigits);
    void ConfigureInches();
    void ConfigureMillimeters();
    void ConfigureTrailingZeros(bool addTrailingZeros);
    double ParseFixed(string value);
    double ParseFloat(string value);
    double ToFixed(double value);
    ClipperOffsetD BuildClipperOffset();
    double GetMaxDeviation();
}
