namespace CADence.Format.Abstractions;

public abstract class ApertureFormatBase(double MitreLimit = 1, int QuadrantSegments = 4)
{
    protected bool fmtConfigured = false;
    protected int nInt;
    protected int nDec;
    protected bool unitConfigured = false;
    protected bool addTrailingZeros = false;
    protected double factor;
    protected bool used = false;
    protected int QuadrantSegments = QuadrantSegments;
    protected double MitreLimit = MitreLimit;

    public abstract void ConfigureFormat(int nInt, int nDec);

    public abstract void ConfigureTrailingZeros(bool addTrailingZeros);

    public abstract void ConfigureInch();

    public abstract void ConfigureMM();

    public abstract double ParseFixed(string s);

    public abstract double ParseFloat(string s);

    public abstract double ToFixed(double d);
    public abstract double FromMM(double i);
    public abstract double ToMM(double i, int digits = 2);

    protected abstract void TryToReconfigure();

    protected abstract void TryToUse();
}
