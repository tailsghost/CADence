namespace CADence.Abstractions.Apertures.Expressions;

/// <summary>
/// Represents a literal numeric expression.
/// </summary>
internal class LiteralExpression : Expression
{
    private readonly double value;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteralExpression"/> class.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    public LiteralExpression(double value)
    {
        this.value = value;
    }

    /// <summary>
    /// Evaluates the literal expression.
    /// </summary>
    /// <param name="vars">A dictionary of variable values.</param>
    /// <returns>The numeric value of the literal.</returns>
    public override double Eval(Dictionary<int, double> vars)
    {
        return value;
    }

    /// <summary>
    /// Returns a string representation of the literal for debugging.
    /// </summary>
    /// <returns>A string representation of the literal.</returns>
    public override string Debug()
    {
        return value.ToString();
    }
}
