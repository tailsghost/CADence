using CADence.Format.Abstractions;
using System.Globalization;

public class ApertureFormat : ApertureFormatBase
{
    private const double InchesToMillimeters = 25.4;
    private const double MillimetersToMillimeters = 1.0;

    private double _conversionFactor;

    public override void ConfigureFormat(int integerDigits, int decimalDigits)
    {
        EnsureReconfigurable();
        _isFormatConfigured = true;
        _integerDigits = integerDigits;
        _decimalDigits = decimalDigits;
    }

    public override void ConfigureTrailingZeros(bool addTrailingZeros)
    {
        EnsureReconfigurable();
        _addTrailingZeros = addTrailingZeros;
    }

    public override void ConfigureInches()
    {
        EnsureReconfigurable();
        _isUnitConfigured = true;
        _conversionFactor = InchesToMillimeters;
    }

    public override void ConfigureMillimeters()
    {
        EnsureReconfigurable();
        _isUnitConfigured = true;
        _conversionFactor = MillimetersToMillimeters;
    }

    public override double ParseFixed(string value)
    {
        EnsureConfigured();
        double val = 0;

        if (value.Contains('.'))
        {
            return ParseFloat(value);
        }

        int totalDigits = value.Length;
        int signOffset = (value[0] == '-' || value[0] == '+') ? 1 : 0;
        int digits = totalDigits - signOffset;

        if (digits <= _integerDigits + _decimalDigits)
        {
            val = double.Parse(value.PadRight(_integerDigits + _decimalDigits + signOffset, '0'), CultureInfo.InvariantCulture);
        }
        else
        {
            throw new FormatException($"Unexpected coordinate length: {value}");
        }

        return AdjustValue(val);
    }

    [Obsolete]
    public override double ParseFixedOld(string value)
    {
        EnsureConfigured();
        if (value.Contains('.'))
        {
            return ParseFloat(value);
        }

        int totalDigits = value.Length;
        int signOffset = (value[0] == '-' || value[0] == '+') ? 1 : 0;
        int digits = totalDigits - signOffset;

        if (digits < _integerDigits + _decimalDigits)
        {
            string paddedString = value.PadRight(_integerDigits + _decimalDigits + signOffset, '0');
            double val = double.Parse(paddedString, CultureInfo.InvariantCulture);
            return AdjustValue(val);
        }
        else if (digits == _integerDigits + _decimalDigits)
        {
            double val = double.Parse(value, CultureInfo.InvariantCulture);
            return AdjustValue(val);
        }
        else
        {
            throw new FormatException($"Unexpected coordinate length: {value}");
        }
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

    private double AdjustValue(double val)
    {
        return val * _conversionFactor;
    }
}
