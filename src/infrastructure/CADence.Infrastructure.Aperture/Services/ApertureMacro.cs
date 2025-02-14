using CADence.Aperture.Expression;
using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.NetTopologySuite;
using CADence.Models.Format.Abstractions;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Services;

public class ApertureMacro : ApertureMacroBase
{
    private static GeometryFactory _geometryFactory = new();
    public override void Append(string cmd)
    {
        if (cmd.StartsWith('$'))
        {
            var parts = cmd.Split('=');
            cmds.Add([Expression.Parse(parts[0]), Expression.Parse(parts[1])]);
        }
        else
        {
            var exprList = new List<Expression>(25);
            var parts = cmd.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                exprList.Add(Expression.Parse(parts[i]));
            }
            cmds.Add(exprList);
        }
    }

    public override ApertureBase Build(List<string> csep, LayerFormatBase format)
    {
        var vars = new Dictionary<int, double>();
        for (int i = 1; i < csep.Count; i++)
        {
            vars[i] = double.Parse(csep[i]);
        }

        var baseAperture = new ApertureBase();
        double code;

        for (var j = 0; j < cmds.Count; j++)
        {
            code = cmds[j][0].Eval(vars);

            switch (code)
            {
                case 1:
                    HandleCircle(cmds[j], vars, baseAperture, format);
                    break;
                case 20:
                    HandleVectorLine(cmds[j], vars, baseAperture, format);
                    break;
                case 21:
                    HandleCenterLine(cmds[j], vars, baseAperture, format);
                    break;
                case 4:
                    HandleOutline(cmds[j], vars, baseAperture, format);
                    break;
                case 5:
                    HandlePolygon(cmds[j], vars, baseAperture, format);
                    break;
                case 6:
                    HandleMoire(cmds[j], vars, baseAperture, format);
                    break;
                case 7:
                    HandleThermal(cmds[j], vars, baseAperture, format);
                    break;
                default:
                    throw new Exception("Invalid aperture macro primitive code");
            }
        }

        var AdditiveGeometry = new ApertureBase();
        AdditiveGeometry.DrawPaths(baseAperture.GetDark());

        return AdditiveGeometry;
    }

    private static void HandleCircle(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase baseAperture, LayerFormatBase fmt)
    {

        if (cmd.Count < 5 || cmd.Count > 6)
            throw new ArgumentException("Invalid circle command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double diameter = Math.Abs(cmd[2].Eval(vars));
        double centerX = cmd[3].Eval(vars);
        double centerY = cmd[4].Eval(vars);
        double rotation = cmd.Count > 5 ? cmd[5].Eval(vars) : 0;

        var geometry = new Point(centerX, centerY).Render(diameter, false);

        baseAperture.DrawPaths(geometry, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    private static void HandleVectorLine(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase baseAperture, LayerFormatBase fmt)
    {

        if (cmd.Count < 7 || cmd.Count > 8)
            throw new ArgumentException("Invalid circle command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double width = Math.Abs(cmd[2].Eval(vars));
        double startX = cmd[3].Eval(vars);
        double startY = cmd[4].Eval(vars);
        double endX = cmd[5].Eval(vars);
        double endY = cmd[6].Eval(vars);
        double rotation = cmd.Count > 7 ? cmd[7].Eval(vars) : 0;

        Coordinate[] coordinates = new Coordinate[] { new Coordinate(startX, startY), new Coordinate(endX, endY) };

        var geometry = new LineString(coordinates).Render(width, true);

        baseAperture.DrawPaths(geometry, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    private static void HandleCenterLine(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase baseAperture, LayerFormatBase fmt)
    {
        if (cmd.Count < 6 || cmd.Count > 7)
            throw new ArgumentException("invalid center line command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double width = Math.Abs(cmd[2].Eval(vars));
        double height = Math.Abs(cmd[3].Eval(vars));
        double centerX = cmd[4].Eval(vars);
        double centerY = cmd[5].Eval(vars);
        double rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;

        var coordinates = new[]
        {
            new Coordinate(centerX + width * 0.5, centerY + height * 0.5),
            new Coordinate(centerX - width * 0.5, centerY + height * 0.5),
            new Coordinate(centerX - width * 0.5, centerY - height * 0.5),
            new Coordinate(centerX + width * 0.5, centerY - height * 0.5),
            new Coordinate(centerX + width * 0.5, centerY + height * 0.5)
        };

        var polygon = _geometryFactory.CreatePolygon(coordinates);

        baseAperture.DrawPaths(polygon, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    private static void HandleOutline(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase baseAperture, LayerFormatBase fmt)
    {

        if (cmd.Count < 3)
            throw new ArgumentException("Invalid outline command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double nVertices = cmd[2].Eval(vars);
        double rotationIndex = 5 + 2 * nVertices;
        double rotation = cmd.Count > (5 + 2 * nVertices) ? cmd.Last().Eval(vars) : 0;

        if (nVertices < 3 || cmd.Count < rotationIndex || cmd.Count > rotationIndex + 1)
            throw new ArgumentException("Invalid outline command in aperture macro");

        var coordinates = new Coordinate[(int)nVertices + 1];

        for (int i = 0; i < nVertices; i++)
        {
            double x = fmt.ToFixed(cmd[3 + 2 * i].Eval(vars));
            double y = fmt.ToFixed(cmd[4 + 2 * i].Eval(vars));
            coordinates[i] = new Coordinate(x, y);
        }

        coordinates[(int)nVertices] = coordinates[0];

        var polygon = _geometryFactory.CreatePolygon(coordinates);

        baseAperture.DrawPaths(polygon, exposure, 0, 0, false, false, rotation / 180 * Math.PI, 1.0, true);
    }

    private static void HandlePolygon(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase baseAperture, LayerFormatBase fmt)
    {

        if (cmd.Count < 6 || cmd.Count > 7)
            throw new ArgumentException("Invalid polygon command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double nVertices = cmd[2].Eval(vars);
        double centerX = cmd[3].Eval(vars);
        double centerY = cmd[4].Eval(vars);
        double diameter = Math.Abs(cmd[5].Eval(vars));
        double rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;

        var coordinates = new Coordinate[(int)nVertices + 1];

        for (int i = 0; i < nVertices; i++)
        {
            double angle = ((double)i / nVertices) * 2.0 * Math.PI;
            double x = centerX + diameter * 0.5 * Math.Cos(angle);
            double y = centerY + diameter * 0.5 * Math.Sin(angle);
            coordinates[i] = new Coordinate(x, y);
        }

        coordinates[(int)nVertices] = coordinates[0];

        var polygon = _geometryFactory.CreatePolygon(coordinates);

        baseAperture.DrawPaths(polygon, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    private static void HandleMoire(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase baseAperture, LayerFormatBase fmt)
    {

    }

    private static void HandleThermal(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase baseAperture, LayerFormatBase fmt)
    {
       
    }
}