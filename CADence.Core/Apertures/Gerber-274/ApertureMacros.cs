using CADence.Abstractions.Apertures;
using CADence.Abstractions.Apertures.Expressions;
using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Core.Apertures.Gerber_274;

public class ApertureMacro : IApertureMacro
{

    public List<Expression> Cmd { get; set; } = [];

    public List<List<Expression>> cmds { get; set; } = [];

    public void Append(string cmd)
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

    public ApertureBase Build(List<string> csep, ILayerFormat format)
    {
        var vars = new Dictionary<int, double>(20);
        for (int i = 1; i < csep.Count; i++)
        {
            vars[i] = double.Parse(csep[i]);
        }

        var baseAperture = new ApertureBase();
        double code;

        for (var j = 0; j < cmds.Count; j++)
        {
            code = (int)Math.Round(cmds[j][0].Eval(vars));

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

        baseAperture.DrawPaths(baseAperture.GetAdditive());

        return baseAperture;
    }

    private static void HandleCircle(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

        if (cmd.Count < 5 || cmd.Count > 6)
            throw new ArgumentException("Invalid circle command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double diameter = Math.Abs(cmd[2].Eval(vars));
        double centerX = cmd[3].Eval(vars);
        double centerY = cmd[4].Eval(vars);
        double rotation = cmd.Count > 5 ? cmd[5].Eval(vars) : 0;


        var paths = new PathsD{ new PathD { new PointD(fmt.ToFixed(centerX), fmt.ToFixed(centerY)) }
             }.Render(fmt.ToFixed(diameter), false, fmt.BuildClipperOffset());

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    private static void HandleVectorLine(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
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

        var paths = new PathsD { new PathD
            {
                new PointD(fmt.ToFixed(startX), fmt.ToFixed(startY)),
                new PointD(fmt.ToFixed(endX), fmt.ToFixed(endY))
            }
            }.Render(fmt.ToFixed(width), true, fmt.BuildClipperOffset());

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    private static void HandleCenterLine(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {
        if (cmd.Count < 6 || cmd.Count > 7)
            throw new ArgumentException("invalid center line command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double width = Math.Abs(cmd[2].Eval(vars));
        double height = Math.Abs(cmd[3].Eval(vars));
        double centerX = cmd[4].Eval(vars);
        double centerY = cmd[5].Eval(vars);
        double rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;

        var paths = new PathsD
        {
            new PathD
            {
                new PointD(fmt.ToFixed(centerX + width * 0.5), fmt.ToFixed(centerY + height * 0.5)),
                new PointD(fmt.ToFixed(centerX - width * 0.5), fmt.ToFixed(centerY + height * 0.5)),
                new PointD(fmt.ToFixed(centerX - width * 0.5), fmt.ToFixed(centerY - height * 0.5)),
                new PointD(fmt.ToFixed(centerX + width * 0.5), fmt.ToFixed(centerY - height * 0.5))
            }
        };

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    private static void HandleOutline(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

        if (cmd.Count < 3)
            throw new ArgumentException("Invalid outline command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        int nVertices = (int)Math.Round(cmd[2].Eval(vars));
        int rotationIndex = 5 + 2 * nVertices;
        double rotation = cmd.Count > (5 + 2 * nVertices) ? cmd.Last().Eval(vars) : 0;

        if (nVertices < 3 || cmd.Count < rotationIndex || cmd.Count > rotationIndex + 1)
            throw new ArgumentException("Invalid outline command in aperture macro");

        var paths = new PathsD(nVertices);

        double x;
        double y;

        for (int i = 0; i < nVertices; i++)
        {
            x = fmt.ToFixed(cmd[3 + 2 * i].Eval(vars));
            y = fmt.ToFixed(cmd[4 + 2 * i].Eval(vars));
            paths.Add([new PointD(x, y)]);
        }

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI, 1.0, true);
    }

    private static void HandlePolygon(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

        if (cmd.Count < 6 || cmd.Count > 7)
            throw new ArgumentException("Invalid polygon command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        int nVertices = (int)Math.Round(cmd[2].Eval(vars));
        double centerX = cmd[3].Eval(vars);
        double centerY = cmd[4].Eval(vars);
        double diameter = Math.Abs(cmd[5].Eval(vars));
        double rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;

        var paths = new PathsD(nVertices);
        double angle;
        double x;
        double y;
        for (int i = 0; i < nVertices; i++)
        {
            angle = (i / nVertices) * 2.0 * Math.PI;
            x = centerX + diameter * 0.5 * Math.Cos(angle);
            y = centerY + diameter * 0.5 * Math.Sin(angle);
            paths.Add([new PointD(fmt.ToFixed(x), fmt.ToFixed(y))]);
        }

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    private static void HandleMoire(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

        
    }

    private static void HandleThermal(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {
       
    }
}
