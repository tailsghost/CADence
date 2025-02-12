using System.Globalization;
using System.Numerics;
using CADence.Models.Format.Abstractions;

namespace CADence.Models.Format;

public class LayerFormat : LayerFormatBase
{
    private const double INCHES_TO_MILLIMETERS = 25.4;
    private const double MILLIMETERS_TO_INCHES = 1.0;

    private double _conversionFactor;

    private bool _addTrailingZeros;

    public override void ConfigureFormat(int integerDigits, int decimalDigits, bool reverse = false)
    {
        EnsureReconfigurable();
        _isFormatConfigured = true;

        if(reverse)
        {
            _integerDigits = decimalDigits;
            _decimalDigits = integerDigits;
        } else
        {
            _integerDigits = integerDigits;
            _decimalDigits = decimalDigits;
        }
    }

    public override void ConfigureInches()
    {
        EnsureReconfigurable();
        _isUnitConfigured = true;
        _conversionFactor = INCHES_TO_MILLIMETERS;
    }

    public override void ConfigureMillimeters()
    {
        EnsureReconfigurable();
        _isUnitConfigured = true;
        _conversionFactor = MILLIMETERS_TO_INCHES;
    }

    public override void ConfigureTrailingZeros(bool addTrailingZeros)
    {
        EnsureReconfigurable();
        _addTrailingZeros = addTrailingZeros;
    }

    public override double ParseFixed(string value)
    {
        if (value.Contains('.'))
        {
            return ParseFloat(value);
        }

        int totalDigits = value.Length;
        int signOffset = (value[0] == '-' || value[0] == '+') ? 1 : 0;
        int digitsCount = totalDigits - signOffset;

        if (digitsCount > _integerDigits + _decimalDigits)
        {
            throw new FormatException($"Unexpected coordinate length: {value}");
        }

        var integerDigits = _integerDigits;

        double parsed = double.Parse(value, CultureInfo.InvariantCulture);
        double result = parsed / Math.Pow(10, integerDigits);

        return AdjustValue(result);
    }


    public override double ParseFloat(string value)
    {
        var result = double.Parse(value, CultureInfo.InvariantCulture);
        return ToFixed(result);
    }

    public override double ToFixed(double value)
    {
        EnsureConfigured();
        return value * _conversionFactor;
    }

    protected override void EnsureReconfigurable()
    {
        if (_isUsed)
        {
            throw new InvalidOperationException("Cannot reconfigure coordinate format after coordinates have already been interpreted.");
        }
    }

    protected override void EnsureConfigured()
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