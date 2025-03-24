using Clipper2Lib;

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
        centerX = center.x;
        centerY = center.y;
        ToPolar(start.x, start.y, out r1, out a1);
        ToPolar(end.x, end.y, out r2, out a2);

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
    public PathD ToCoordinates()
    {
        const double angleStep = 0.05;
        int nVertices = (int)Math.Ceiling(Math.Abs(a2 - a1) / angleStep);
        if (nVertices < 1)
            nVertices = 1;

        PathD coords = new PathD(nVertices + 1);
        for (int i = 0; i <= nVertices; i++)
        {
            double fraction = (double)i / nVertices;
            double currentAngle = a1 + fraction * (a2 - a1);
            double currentRadius = r1 + fraction * (r2 - r1);
            double x = centerX + currentRadius * Math.Cos(currentAngle);
            double y = centerY + currentRadius * Math.Sin(currentAngle);
            coords.Add(new PointD(x, y));
        }
        return coords;
    }
}
