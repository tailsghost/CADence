using System;
using System.Collections.Generic;

namespace CADence.Aperture.Expression;

public class Token : Expression
{
    private readonly char token;

    public Token(char token)
    {
        this.token = token;
    }

    public override double Eval(Dictionary<int, double> vars)
    {
        throw new InvalidOperationException("Cannot evaluate token");
    }

    public override char GetToken()
    {
        return token;
    }

    public override string Debug()
    {
        return token.ToString();
    }
}
