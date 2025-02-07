using RTools_NTS.Util;
using System.Linq.Expressions;

namespace CADence.Aperture.Expression;

public abstract class Expression
{
    public abstract double Eval(Dictionary<int, double> vars);
    public abstract string Debug();
    public virtual char GetToken() => '\0';

    public static Expression Reduce(List<Expression> expr)
    {
        if (expr.Count == 0)
        {
            throw new Exception("empty aperture macro (sub)expression");
        }

        int level;
        for (int i = 0; i < expr.Count; i++)
        {
            if (expr[i].GetToken() == '(')
            {
                level = 1;
                for (int j = i + 1; j < expr.Count; j++)
                {
                    char t = expr[j].GetToken();
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

        char oper;
        for (int i = 0; i < expr.Count - 1; i++)
        {
            if (expr[i + 1].GetToken() == '\0')
            {
                oper = expr[i].GetToken();
                if (oper == '-' || oper == '+')
                {
                    expr[i] = new UnaryExpression(oper, expr[i + 1]);
                    expr.RemoveAt(i + 1);
                }
            }
        }

        for (int i = 1; i < expr.Count - 1; i++)
        {
            oper = expr[i].GetToken();
            if (oper == 'x' || oper == '/' || oper == '+' || oper == '-')
            {
                expr[i - 1] = new BinaryExpression(oper, expr[i - 1], expr[i + 1]);
                expr.RemoveRange(i, 2);
                i--;
            }
        }

        if (expr.Count != 1)
        {
            throw new Exception("invalid expression");
        }

        return expr[0];
    }

    public static Expression Parse(string expr)
    {
        List<Expression> tokens = new();
        string currentToken = "";
        bool isNumber = false;

        foreach (char c in expr + " ")
        {
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
                else if (c == '+' || c == '-' || c == 'x' || c == '/' || c == '(' || c == ')')
                {
                    tokens.Add(new Token(c));
                }
            }
        }

        return Reduce(tokens);
    }
}
