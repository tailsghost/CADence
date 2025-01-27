namespace CADence.Aperture.Expression;

internal class BinaryExpression : Expression
{
    private readonly char oper;
    private readonly Expression lhs, rhs;

    public BinaryExpression(char oper, Expression lhs, Expression rhs)
    {
        this.oper = oper;
        this.lhs = lhs;
        this.rhs = rhs;
    }

    internal override double Eval(Dictionary<int, double> vars)
    {
        switch (oper)
        {
            case '+': return lhs.Eval(vars) + rhs.Eval(vars);
            case '-': return lhs.Eval(vars) - rhs.Eval(vars);
            case 'x': return lhs.Eval(vars) * rhs.Eval(vars);
            case '/': return lhs.Eval(vars) / rhs.Eval(vars);
            default: throw new Exception("Invalid operator");
        }
    }

    internal override string Debug()
    {
        return lhs.Debug() + " " + oper + " " + rhs.Debug();
    }
}
