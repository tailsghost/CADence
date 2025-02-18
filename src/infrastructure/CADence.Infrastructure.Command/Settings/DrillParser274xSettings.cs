using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.Gerber_274x;
using CADence.Infrastructure.Aperture.NetTopologySuite;
using CADence.Infrastructure.Command.Property.Gerber274x;
using CADence.Infrastructure.Parser.Abstractions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;

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
    public Drill Aperture;

    /// <summary>
    /// Список всех апертур
    /// </summary>
    public Dictionary<int, Drill> Apertures = [];

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

        var d = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));
        if (d >= 2 * radius)
        {
            throw new ArgumentException("Расстояние между точками должно быть меньше диаметра окружности.");
        }

        var e = Math.Sqrt(1 - (d * d) / (4 * radius * radius));

        var cx = (x0 + x1) / 2 + e * (y1 - y0) / d;
        var cy = (y0 + y1) / 2 - e * (x1 - x0) / d;

        var angle0 = Math.Atan2(y0 - cy, x0 - cx);
        var angle1 = Math.Atan2(y1 - cy, x1 - cx);

        if (ccw && angle1 < angle0) angle1 += 2 * Math.PI;
        if (!ccw && angle0 < angle1) angle0 += 2 * Math.PI;

        var shapeFactory = new GeometricShapeFactory
        {
            Base = new Coordinate(cx, cy),
            Size = radius * 2 
        };

        var arc = shapeFactory.CreateArc(angle0, angle1);

        var coordinates = arc.Coordinates;

        foreach (var coord in coordinates)
        {
            Points.Add(new Point(coord.X, coord.Y));
        }
    }
}