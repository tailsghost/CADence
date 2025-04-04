using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using System.Globalization;
using ExtensionClipper2;

namespace CADence.Core.Formats;

/// <summary>
/// Implements ILayerFormat for coordinate conversion and formatting.
/// </summary>
internal class LayerFormat : ILayerFormat
{
    private const double INCHES_TO_MILLIMETERS = 25.4;
    private const double MILLIMETERS_TO_INCHES = 1.0;
    private double MAX_DEVIATION;
    private double MITER_LIMIT;
    private bool _isFormatConfigured = false;
    private int _integerDigits;
    private int _decimalDigits;
    private bool _isUnitConfigured = false;
    private bool _isUsed = false;
    private bool _addTrailingZeros = false;

    private double _conversionFactor;


    public LayerFormat(double maxDeviation = 0.01, double miterLimit = 0.75)
    {
        MAX_DEVIATION = maxDeviation;
        MITER_LIMIT = miterLimit;
    }

    /// <summary>
    /// Configures the coordinate format.
    /// </summary>
    public void ConfigureFormat(int integerDigits, int decimalDigits)
    {
        EnsureReconfigurable();
        _isFormatConfigured = true;
        _integerDigits = integerDigits;
        _decimalDigits = decimalDigits;
    }

    /// <summary>
    /// Configures the unit conversion to inches.
    /// </summary>
    public void ConfigureInches()
    {
        EnsureReconfigurable();
        _isUnitConfigured = true;
        _conversionFactor = INCHES_TO_MILLIMETERS;
    }

    /// <summary>
    /// Configures the unit conversion to millimeters.
    /// </summary>
    public void ConfigureMillimeters()
    {
        EnsureReconfigurable();
        _isUnitConfigured = true;
        _conversionFactor = MILLIMETERS_TO_INCHES;
    }

    /// <summary>
    /// Configures whether to add trailing zeros to the parsed coordinates.
    /// </summary>
    public void ConfigureTrailingZeros(bool addTrailingZeros)
    {
        EnsureReconfigurable();
        _addTrailingZeros = addTrailingZeros;
    }

    /// <summary>
    /// Parses a fixed-point number from a string.
    /// </summary>
    public double ParseFixed(string value)
    {
        EnsureConfigured();

        if (value.Contains('.'))
        {
            return ParseFloat(value);
        }

        var isNegative = value[0] == '-' || value[0] == '+';
        var digits = isNegative ? value.Substring(1) : value;

        var expectedLength = _integerDigits + _decimalDigits;

        if (_addTrailingZeros && digits.Length < expectedLength)
        {
            digits = digits.PadRight(expectedLength, '0');
        }

        else if (digits.Length > expectedLength)
        {
            throw new FormatException($"Unexpected coordinate length: {value}");
        }

        string resultStr;
        if (digits.Length == expectedLength)
        {
            var intPart = digits.Substring(0, _integerDigits);
            var fracPart = digits.Substring(_integerDigits);
            resultStr = intPart + "." + fracPart;
        }
        else
        {
            var pos = digits.Length - _decimalDigits;
            if (pos <= 0)
            {
                resultStr = "0." + new string('0', -pos) + digits;
            }
            else
            {
                resultStr = digits.Substring(0, pos) + "." + digits.Substring(pos);
            }
        }

        if (isNegative)
        {
            if (resultStr[0] != '-')
            {
                resultStr = "-" + resultStr;
            }
        }

        var result = double.Parse(resultStr, CultureInfo.InvariantCulture);

        return AdjustValue(result);
    }


    /// <summary>
    /// Parses a floating-point number from a string.
    /// </summary>
    public double ParseFloat(string value)
    {
        var result = double.Parse(value, CultureInfo.InvariantCulture);
        return ToFixed(result);
    }


    /// <summary>
    /// Converts a coordinate value using the conversion factor.
    /// </summary>
    public double ToFixed(double value)
    {
        EnsureConfigured();
        return value * _conversionFactor;
    }

    /// <summary>
    /// Builds a new ClipperOffsetD object.
    /// </summary>
    public ClipperOffsetD BuildClipperOffset()
        => new ClipperOffsetD(MITER_LIMIT, MAX_DEVIATION);


    private void EnsureReconfigurable()
    {
        if (_isUsed)
        {
            throw new InvalidOperationException("Cannot reconfigure coordinate format after coordinates have already been interpreted.");
        }
    }

    private void EnsureConfigured()
    {
        if (!_isFormatConfigured || !_isUnitConfigured)
        {
            throw new InvalidOperationException("Cannot convert coordinates before format or unit is configured.");
        }
        _isUsed = true;
    }

    /// <summary>
    /// Applies the conversion factor to the given value.
    /// </summary>
    private double AdjustValue(double value)
    {
        return value * _conversionFactor;
    }

    public double GetMaxDeviation()
        => MAX_DEVIATION;
}