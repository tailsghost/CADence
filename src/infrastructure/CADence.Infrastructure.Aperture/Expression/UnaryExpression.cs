namespace CADence.Aperture.Expression;

public class UnaryExpression : Expression
{
    private readonly char oper;
    private readonly Expression expr;

    public UnaryExpression(char oper, Expression expr)
    {
        this.oper = oper;
        this.expr = expr;
    }

    public override double Eval(Dictionary<int, double> vars)
    {
        return oper == '+' ? expr.Eval(vars) : -expr.Eval(vars);
    }

    public override string Debug()
    {
        return oper + expr.Debug();
    }
}
