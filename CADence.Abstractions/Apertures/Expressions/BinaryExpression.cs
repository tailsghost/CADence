namespace CADence.Abstractions.Apertures.Expressions;

public class BinaryExpression : Expression
{
    private readonly char oper;
    private readonly Expression lhs, rhs;

    public BinaryExpression(char oper, Expression lhs, Expression rhs)
    {
        this.oper = oper;
        this.lhs = lhs;
        this.rhs = rhs;
    }

    public override double Eval(Dictionary<int, double> vars)
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

    public override string Debug()
    {
        return lhs.Debug() + " " + oper + " " + rhs.Debug();
    }
}
