using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;
using System.Globalization;

namespace CADence.Core.Formats;

public class LayerFormat(double maxDeviation = 0.0001, double miterLimit = 1.0) : ILayerFormat
{
    private const double INCHES_TO_MILLIMETERS = 25.4;
    private const double MILLIMETERS_TO_INCHES = 1.0;
    private double MAX_DEVIATION = maxDeviation;
    private double MITER_LIMIT = miterLimit;
    private bool _isFormatConfigured = false;
    private int _integerDigits;
    private int _decimalDigits;
    private bool _isUnitConfigured = false;
    private bool _isUsed = false;
    private bool _addTrailingZeros = false;

    private double _conversionFactor;

    public void ConfigureFormat(int integerDigits, int decimalDigits)
    {
        EnsureReconfigurable();
        _isFormatConfigured = true;

        _integerDigits = integerDigits;
        _decimalDigits = decimalDigits;

    }

    public void ConfigureInches()
    {
        EnsureReconfigurable();
        _isUnitConfigured = true;
        _conversionFactor = INCHES_TO_MILLIMETERS;
    }

    public void ConfigureMillimeters()
    {
        EnsureReconfigurable();
        _isUnitConfigured = true;
        _conversionFactor = MILLIMETERS_TO_INCHES;
    }

    public void ConfigureTrailingZeros(bool addTrailingZeros)
    {
        EnsureReconfigurable();
        _addTrailingZeros = addTrailingZeros;
    }

    public double ParseFixed(string value)
    {
        EnsureConfigured();

        if (value.Contains('.'))
        {
            return ParseFloat(value);
        }

        bool isNegative = value[0] == '-' || value[0] == '+';
        string digits = isNegative ? value.Substring(1) : value;

        int expectedLength = _integerDigits + _decimalDigits;

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
            string intPart = digits.Substring(0, _integerDigits);
            string fracPart = digits.Substring(_integerDigits);
            resultStr = intPart + "." + fracPart;
        }
        else
        {
            int pos = digits.Length - _decimalDigits;
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

        double result = double.Parse(resultStr, CultureInfo.InvariantCulture);

        return AdjustValue(result);
    }


    public double ParseFloat(string value)
    {
        var result = double.Parse(value, CultureInfo.InvariantCulture);
        return ToFixed(result);
    }

    public double ToFixed(double value)
    {
        EnsureConfigured();
        return value * _conversionFactor;
    }

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
    /// Применяет коэффициент преобразования к переданному значению.
    /// </summary>
    /// <param name="value">Число, к которому применяется коэффициент преобразования.</param>
    /// <returns>Преобразованное значение, полученное умножением на коэффициент.</returns>
    private double AdjustValue(double value)
    {
        return value * _conversionFactor;
    }
}