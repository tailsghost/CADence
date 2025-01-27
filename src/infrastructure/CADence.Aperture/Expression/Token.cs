namespace CADence.Aperture.Expression;

internal class Token : Expression
{
    private readonly char token;

    public Token(char token)
    {
        this.token = token;
    }

    internal override double Eval(Dictionary<int, double> vars)
    {
        throw new InvalidOperationException("Cannot evaluate token");
    }

    internal override char GetToken()
    {
        return token;
    }

    internal override string Debug()
    {
        return token.ToString();
    }
}
