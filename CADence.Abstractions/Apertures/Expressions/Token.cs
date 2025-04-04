namespace CADence.Abstractions.Apertures.Expressions;

/// <summary>
/// Represents a token in an expression.
/// </summary>
internal class Token : Expression
{
    private readonly char token;

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class.
    /// </summary>
    /// <param name="token">The token character.</param>
    public Token(char token)
    {
        this.token = token;
    }

    /// <summary>
    /// Evaluates the token. Throws an exception because tokens cannot be evaluated directly.
    /// </summary>
    /// <param name="vars">A dictionary of variable values.</param>
    /// <returns>Never returns normally.</returns>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    public override double Eval(Dictionary<int, double> vars)
    {
        throw new InvalidOperationException("Cannot evaluate token");
    }

    /// <summary>
    /// Gets the token character.
    /// </summary>
    /// <returns>The token character.</returns>
    public override char GetToken()
    {
        return token;
    }

    /// <summary>
    /// Returns a string representation of the token.
    /// </summary>
    /// <returns>A string representation of the token.</returns>
    public override string Debug()
    {
        return token.ToString();
    }
}
