namespace CADence.Abstractions.Apertures.Expressions;

/// <summary>
/// Abstract base class for all expressions.
/// </summary>
public abstract class Expression
{
    /// <summary>
    /// Evaluates the expression using the specified variable values.
    /// </summary>
    /// <param name="vars">A dictionary of variable values.</param>
    /// <returns>The evaluated value.</returns>
    public abstract double Eval(Dictionary<int, double> vars);

    /// <summary>
    /// Returns a string representation of the expression for debugging purposes.
    /// </summary>
    /// <returns>A string representation of the expression.</returns>
    public abstract string Debug();

    /// <summary>
    /// Gets the token character of the expression, if applicable.
    /// </summary>
    /// <returns>The token character, or '\0' if not applicable.</returns>
    public virtual char GetToken() => '\0';


    /// <summary>
    /// Reduces a list of expressions into a single expression.
    /// </summary>
    /// <param name="expr">The list of expressions to reduce.</param>
    /// <returns>The reduced expression.</returns>
    public static Expression Reduce(List<Expression> expr)
    {
        if (expr.Count == 0)
        {
            new Exception("empty aperture macro (sub)expression");
        }

        for (var i = 0; i < expr.Count; i++)
        {
            if (expr[i].GetToken() == '(')
            {
                var level = 1;
                for (var j = i + 1; j < expr.Count; j++)
                {
                    var t = expr[j].GetToken();
                    if (t == '(') level++;
                    if (t == ')') level--;
                    if (level == 0)
                    {
                        var subExpr = Reduce(expr.Skip(i + 1).Take(j - i - 1).ToList());
                        expr[i] = subExpr;
                        expr.RemoveRange(i + 1, j - i);
                        break;
                    }
                }
            }
        }

        for (var i = 0; i < expr.Count - 1; i++)
        {
            if (expr[i + 1].GetToken() == '\0')
            {
                var oper = expr[i].GetToken();
                if (oper == '-' || oper == '+')
                {
                    expr[i] = new UnaryExpression(oper, expr[i + 1]);
                    expr.RemoveAt(i + 1);
                }
            }
        }

        for (var i = 1; i < expr.Count - 1; i++)
        {
            var oper = expr[i].GetToken();
            if (oper == 'x' || oper == '/' || oper == '+' || oper == '-')
            {
                expr[i - 1] = new BinaryExpression(oper, expr[i - 1], expr[i + 1]);
                expr.RemoveRange(i, 2);
                i--;
            }
        }

        if (expr.Count != 1)
        {
            new Exception("invalid expression");
        }

        return expr[0];
    }

    /// <summary>
    /// Parses a string into an <see cref="Expression"/>.
    /// </summary>
    /// <param name="expr">The string expression to parse.</param>
    /// <returns>The parsed expression.</returns>
    public static Expression Parse(string expr)
    {
        List<Expression> tokens = new();
        var currentToken = "";
        var isNumber = false;

        for (var i = 0; i <= expr.Length; i++)
        {
            var c = (i < expr.Length) ? expr[i] : ' ';

            if (char.IsDigit(c) || c == '.')
            {
                currentToken += c;
                isNumber = true;
            }
            else
            {
                if (isNumber)
                {
                    tokens.Add(new LiteralExpression(double.Parse(currentToken, System.Globalization.CultureInfo.InvariantCulture)));
                    currentToken = "";
                    isNumber = false;
                }

                if (c == '$')
                {
                    currentToken += c;
                }
                else if (c is '+' or '-' or 'x' or '/' or '(' or ')')
                {
                    tokens.Add(new Token(c));
                }
            }
        }

        return Reduce(tokens);
    }
}
