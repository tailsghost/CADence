using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.NetTopologySuite;
using CADence.Infrastructure.Command.Property.Gerber274x;
using CADence.Infrastructure.Parser.Abstractions;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Parser.Settings;

/// <summary>
/// Настройки для парсера Drill 274X.
/// Наследует базовые параметры Drill-парсера.
/// </summary>
public class DrillParser274xSettings : DrillParserSettingsBase
{

    private readonly GeometryFactory _geomFactory = new GeometryFactory();

    /// <summary>
    /// Текущая апертура
    /// </summary>
    public ApertureBase Aperture;

    /// <summary>
    /// Список всех апертур
    /// </summary>
    public Dictionary<int, ApertureBase> Apertures = [];

    /// <summary>
    /// Текущий макрос апертуры.
    /// </summary>
    public ApertureMacroBase AmBuilder;

    /// <summary>
    /// Список инструментов
    /// </summary>
    public Dictionary<int, Tool> Tools = [];

    /// <summary>
    /// Текущий инструмент
    /// </summary>
    public Tool Tool;

    /// <summary>
    /// Функция для установки дырки на плату
    /// </summary>
    public void CommitPath()
    {
        if (Tool == null)
        {
            throw new InvalidOperationException("tool use before any tool is selected");
        }

        MinHole = Math.Min(MinHole, Tool.diameter);

        if (Tool.plated)
        {
            if (Points.Count == 1)
            {
                Pth.DrawPaths(Points[0].Render(Tool.diameter, false));
            }
            else if (Points.Count == 2)
            {
                var line = new LineString(new[]
                {
                    new Coordinate(Points[0].X, Points[0].Y),
                    new Coordinate(Points[1].X, Points[1].Y)
                });

                Pth.DrawPaths(line.Render(Tool.diameter, false));
            }
            else if (Points.Count > 2)
            {
                Coordinate[] coords = new Coordinate[Points.Count];
                for (int i = 0; i < Points.Count; i++)
                {
                    coords[i] = new Coordinate(Points[i].X, Points[i].Y);
                }
                Pth.DrawPaths(_geomFactory.CreateLinearRing(coords).Render(Tool.diameter, false));
            }
        }
        else
        {
            if (Points.Count == 1)
            {
                Pth.DrawPaths(Points[0].Render(Tool.diameter, false));
            }
            else if (Points.Count == 2)
            {
                var line = new LineString(new[]
                {
                    new Coordinate(Points[0].X, Points[0].Y),
                    new Coordinate(Points[1].X, Points[1].Y)
                });

                Npth.DrawPaths(line.Render(Tool.diameter, false));
            }
            else if (Points.Count > 2)
            {
                Coordinate[] coords = new Coordinate[Points.Count];
                for (int i = 0; i < Points.Count; i++)
                {
                    coords[i] = new Coordinate(Points[i].X, Points[i].Y);
                }
                Npth.DrawPaths(_geomFactory.CreateLinearRing(coords).Render(Tool.diameter, false));
            }
        }

        Points.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="parseFixed"></param>
    /// <param name="ccw"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void AddArc(Point startPoint, Point endPoint, double radius, bool ccw)
    {
        var x0 = startPoint.X;
        var y0 = startPoint.Y;
        var x1 = endPoint.X;
        var y1 = endPoint.Y;
        var r = radius;

        var d = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));
        var e = 2.0 * r / d;
        e = (e < 1.0) ? 0.0 : Math.Sqrt(e * e - 1.0) * (ccw ? 1 : -1);

        var ax = (x0 - x1) / 2;
        var ay = (y0 - y1) / 2;
        var xc = ax + ay * e;
        var yc = ay - ax * e;

        var a0 = Math.Atan2(y0 - yc, x0 - xc);
        var a1 = Math.Atan2(y1 - yc, x1 - xc);
        if (ccw && a1 < a0) a1 += 2.0 * Math.PI;
        if (!ccw && a0 < a1) a0 += 2.0 * Math.PI;

        var epsilon = 1; // В дальнейшем поменять это значение на валидное
        var f = (r > epsilon) ? (1.0 - epsilon / r) : 0.0;
        var th = Math.Acos(2.0 * f * f - 1.0) + 1e-3;
        var nVertices = Math.Ceiling(Math.Abs(a1 - a0) / th);

        for (int i = 1; i <= nVertices; i++)
        {
            var f1 = i / nVertices;
            var f0 = 1.0 - f1;
            var va = f0 * a0 + f1 * a1;
            var vx = xc + r * Math.Cos(va);
            var vy = yc + r * Math.Sin(va);
            Points.Add(new Point(Math.Round(vx), Math.Round(vy)));
        }
    }
}