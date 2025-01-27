using CADence.Format.Abstractions;
using System.Globalization;

namespace CADence.Format.Format;

internal class Format : FormatBase
{
    public override void ConfigureFormat(int nInt, int nDec)
    {
        TryToReconfigure();
        fmtConfigured = true;
        this.nInt = nInt;
        this.nDec = nDec;
    }

    public override void ConfigureTrailingZeros(bool addTrailingZeros)
    {
        TryToReconfigure();
        this.addTrailingZeros = addTrailingZeros;
    }

    public override void ConfigureInch()
    {
        TryToReconfigure();
        unitConfigured = true;
        factor = 25.4;
    }

    public override void ConfigureMM()
    {
        TryToReconfigure();
        unitConfigured = true;
        factor = 1.0;
    }

    public override double ParseFixed(string s)
    {
        TryToUse();
        if (s.Contains('.'))
        {
            return ParseFloat(s);
        }

        int totalDigits = s.Length;
        int signOffset = (s[0] == '-' || s[0] == '+') ? 1 : 0;
        int digits = totalDigits - signOffset;

        if (digits < nInt + nDec)
        {
            string paddedString = s.PadRight(nInt + nDec + signOffset, '0');
            double val = double.Parse(paddedString, CultureInfo.InvariantCulture);
            return AdjustValue(val);
        }
        else if (digits == nInt + nDec)
        {
            double val = double.Parse(s, CultureInfo.InvariantCulture);
            return AdjustValue(val);
        }
        else
        {
            throw new FormatException($"Unexpected coordinate length: {s}");
        }
    }

    public override double ParseFloat(string s)
    {
        var result = double.Parse(s, CultureInfo.InvariantCulture);
        return ToFixed(result);
    }

    public override double ToFixed(double d)
    {
        TryToUse();
        return Math.Round(d * factor, nDec);
    }

    public override double FromMM(double i)
    {
        return Math.Round(i * nDec);
    }

    public override double ToMM(double i, int digits = 2)
    {
        return Math.Round(i / nDec, digits);
    }

    protected override void TryToReconfigure()
    {
        if (used)
        {
            throw new InvalidOperationException(
                "Cannot reconfigure coordinate format after coordinates have already been interpreted."
            );
        }
    }

    protected override void TryToUse()
    {
        if (!fmtConfigured)
        {
            throw new InvalidOperationException(
                "Cannot convert coordinates before coordinate format is configured."
            );
        }
        if (!unitConfigured)
        {
            throw new InvalidOperationException(
                "Cannot convert coordinates before unit is configured."
            );
        }

        used = true;
    }

    private double AdjustValue(double val)
    {
        if (factor == 25.4)
        {
            return val * 254; 
        }
        else if (factor == 1.0)
        {
            return val; 
        }
        else
        {
            throw new InvalidOperationException("Unknown conversion factor");
        }
    }
}
