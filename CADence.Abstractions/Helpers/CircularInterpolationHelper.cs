using ExtensionClipper2.Core;
using System.IO;
using System.Runtime.Intrinsics.X86;

namespace CADence.Abstractions.Helpers;

public class CircularInterpolationHelper
{
    private double centerX, centerY;
    private double r1, r2;
    private double a1, a2;

    /// <summary>
    /// Преобразует координаты (x, y) в полярные относительно центра (centerX, centerY)
    /// </summary>
    private void ToPolar(double x, double y, out double r, out double a)
    {
        x -= centerX;
        y -= centerY;
        r = Math.Sqrt(x * x + y * y);
        a = Math.Atan2(y, x);
    }

    /// <summary>
    /// Конструктор задаёт дугу между точками start и end с указанным центром.
    /// Параметры ccw и multi определяют направление (против часовой стрелки, ccw)
    /// и режим расчёта (multi – многоквадрантная аппроксимация).
    /// </summary>
    public CircularInterpolationHelper(PointD start, PointD end, PointD center, bool ccw, bool multi)
    {
        centerX = center.X;
        centerY = center.Y;
        ToPolar(start.X, start.Y, out r1, out a1);
        ToPolar(end.X, end.Y, out r2, out a2);

        if (multi)
        {
            if (ccw)
            {
                if (a2 <= a1)
                    a2 += 2.0 * Math.PI;
            }
            else
            {
                if (a1 <= a2)
                    a1 += 2.0 * Math.PI;
            }
        }
        else
        {
            if (ccw)
            {
                if (a2 < a1)
                    a2 += 2.0 * Math.PI;
            }
            else
            {
                if (a1 < a2)
                    a1 += 2.0 * Math.PI;
            }
        }
    }

    /// <summary>
    /// Возвращает true, если дуга лежит в пределах одного квадранта.
    /// </summary>
    public bool IsSingleQuadrant()
    {
        return Math.Abs(a1 - a2) <= Math.PI / 2 + 1e-3;
    }

    /// <summary>
    /// Для сравнения – возвращает максимальный из радиусов.
    /// </summary>
    public double Error()
    {
        return Math.Max(r1, r2);
    }

    /// <summary>
    /// Создает аппроксимирующий LineString для дуги, используя фиксированный шаг по углу.
    /// </summary>
    public PathD ToCoordinates(double epsilon)
    {
        var r = (r1 + r2) * 0.5;
        var x = (r > epsilon) ? (1.0 - epsilon / r) : 0.0;
        var th = Math.Acos(2.0 * x * x - 1.0) + 1e-3;
        var nVertices = Math.Ceiling(Math.Abs(a2 - a1) / th);
        PathD p = new((int)nVertices);

        for (int i = 0; i <= nVertices; i++)
        {
            var f2 = i / nVertices;
            var f1 = 1.0 - f2;
            var vr = f1 * r1 + f2 * r2;
            var va = f1 * a1 + f2 * a2;
            var vx = centerX + vr * Math.Cos(va);
            var vy = centerY + vr * Math.Sin(va);
            p.Add(new PointD(vx, vy));
        }

        return p;
    }
}
