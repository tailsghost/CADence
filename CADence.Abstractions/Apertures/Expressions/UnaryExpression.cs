namespace CADence.Abstractions.Apertures.Expressions;

/// <summary>
/// Represents a unary expression.
/// </summary>
internal class UnaryExpression : Expression
{
    private readonly char oper;
    private readonly Expression expr;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnaryExpression"/> class.
    /// </summary>
    /// <param name="oper">The unary operator.</param>
    /// <param name="expr">The expression to which the operator is applied.</param>
    public UnaryExpression(char oper, Expression expr)
    {
        this.oper = oper;
        this.expr = expr;
    }

    /// <summary>
    /// Evaluates the expression with the specified variable values.
    /// </summary>
    /// <param name="vars">A dictionary of variable values.</param>
    /// <returns>The evaluated value.</returns>
    public override double Eval(Dictionary<int, double> vars)
    {
        return oper == '+' ? expr.Eval(vars) : -expr.Eval(vars);
    }

    /// <summary>
    /// Returns a string representation of the expression for debugging purposes.
    /// </summary>
    /// <returns>A string representation of the expression.</returns>
    public override string Debug()
    {
        return oper + expr.Debug();
    }
}
