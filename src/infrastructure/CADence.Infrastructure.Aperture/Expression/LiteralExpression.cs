namespace CADence.Aperture.Expression;

internal class LiteralExpression : Expression
{
    private readonly double value;

    public LiteralExpression(double value)
    {
        this.value = value;
    }

    internal override double Eval(Dictionary<int, double> vars)
    {
        return value;
    }

    internal override string Debug()
    {
        return value.ToString();
    }
}
