namespace CADence.Abstractions.Apertures.Expressions;

/// <summary>
/// Represents a binary expression with an operator between two expressions.
/// </summary>
internal class BinaryExpression : Expression
{
    private readonly char oper;
    private readonly Expression lhs, rhs;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryExpression"/> class.
    /// </summary>
    /// <param name="oper">The binary operator.</param>
    /// <param name="lhs">The left-hand side expression.</param>
    /// <param name="rhs">The right-hand side expression.</param>
    public BinaryExpression(char oper, Expression lhs, Expression rhs)
    {
        this.oper = oper;
        this.lhs = lhs;
        this.rhs = rhs;
    }

    /// <summary>
    /// Evaluates the binary expression using the provided variable values.
    /// </summary>
    /// <param name="vars">A dictionary of variable values.</param>
    /// <returns>The evaluated result.</returns>
    public override double Eval(Dictionary<int, double> vars)
    {
        return oper switch
        {
            '+' => lhs.Eval(vars) + rhs.Eval(vars),
            '-' => lhs.Eval(vars) - rhs.Eval(vars),
            'x' => lhs.Eval(vars) * rhs.Eval(vars),
            '/' => lhs.Eval(vars) / rhs.Eval(vars),
            _ => throw new Exception("Invalid operator")
        };
    }

    /// <summary>
    /// Returns a string representation of the binary expression for debugging purposes.
    /// </summary>
    /// <returns>A string representing the binary expression.</returns>
    public override string Debug()
    {
        return lhs.Debug() + " " + oper + " " + rhs.Debug();
    }
}
