using CADence.Abstractions.Apertures;
using CADence.Abstractions.Apertures.Expressions;
using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using ExtensionClipper2.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Apertures.Gerber_274;

/// <summary>
/// Implementation of the aperture macro interface.
/// Processes and builds an aperture based on macro commands.
/// </summary>
internal class ApertureMacro : IApertureMacro
{
    private IServiceProvider _provider;
    public ApertureMacro(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Collection of parsed expressions.
    /// </summary>
    public List<Expression> Cmd { get; set; } = new();

    /// <summary>
    /// Collection of lists of parsed expressions.
    /// </summary>
    public List<List<Expression>> cmds { get; set; } = new();

    /// <summary>
    /// Appends a command to the macro. If the command starts with '$', splits it into two expressions.
    /// Otherwise, splits by commas.
    /// </summary>
    /// <param name="cmd">The command string.</param>
    public void Append(string cmd)
    {
        if (cmd.StartsWith('$'))
        {
            var parts = cmd.Split('=');
            cmds.Add(new List<Expression> { Expression.Parse(parts[0]), Expression.Parse(parts[1])});
        }
        else
        {
            var exprList = new List<Expression>(25);
            var parts = cmd.Split(',');
            for (var i = 0; i < parts.Length; i++)
            {
                exprList.Add(Expression.Parse(parts[i]));
            }
            cmds.Add(exprList);
        }
    }

    /// <summary>
    /// Builds the aperture based on the provided parameters and format.
    /// Evaluates expressions from the command lists and calls specific handlers based on the code.
    /// </summary>
    /// <param name="csep">The list of string parameters.</param>
    /// <param name="format">The aperture format.</param>
    /// <returns>The constructed aperture.</returns>
    public ApertureBase Build(List<string> csep, ILayerFormat format)
    {
        var vars = new Dictionary<int, double>(20);
        for (var i = 1; i < csep.Count; i++)
        {
            vars[i] = double.Parse(csep[i]);
        }

        var baseAperture = _provider.GetRequiredService<Unknown>();

        for (var j = 0; j < cmds.Count; j++)
        {
            var code = (int)Math.Round(cmds[j][0].Eval(vars));

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

    /// <summary>
    /// Handles the circle command for aperture macro.
    /// </summary>
    private static void HandleCircle(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

        if (cmd.Count is < 5 or > 6)
            throw new ArgumentException("Invalid circle command in aperture macro");

        var exposure = cmd[1].Eval(vars) > 0.5;
        var diameter = Math.Abs(cmd[2].Eval(vars));
        var centerX = cmd[3].Eval(vars);
        var centerY = cmd[4].Eval(vars);
        var rotation = cmd.Count > 5 ? cmd[5].Eval(vars) : 0;

        var paths = new PathsD{ new PathD { new PointD(fmt.ToFixed(centerX), fmt.ToFixed(centerY)) }
                            }.Render(fmt.ToFixed(diameter), false, fmt.BuildClipperOffset());

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    /// <summary>
    /// Handles the vector line command for aperture macro.
    /// </summary>
    private static void HandleVectorLine(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

        if (cmd.Count is < 7 or > 8)
            throw new ArgumentException("Invalid circle command in aperture macro");

        var exposure = cmd[1].Eval(vars) > 0.5;
        var width = Math.Abs(cmd[2].Eval(vars));
        var startX = cmd[3].Eval(vars);
        var startY = cmd[4].Eval(vars);
        var endX = cmd[5].Eval(vars);
        var endY = cmd[6].Eval(vars);
        var rotation = cmd.Count > 7 ? cmd[7].Eval(vars) : 0;

        var paths = new PathsD { new PathD
            {
                new PointD(fmt.ToFixed(startX), fmt.ToFixed(startY)),
                new PointD(fmt.ToFixed(endX), fmt.ToFixed(endY))
            }
            }.Render(fmt.ToFixed(width), true, fmt.BuildClipperOffset());

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    /// <summary>
    /// Handles the center line command for aperture macro.
    /// </summary>
    private static void HandleCenterLine(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {
        if (cmd.Count is < 6 or > 7)
            throw new ArgumentException("invalid center line command in aperture macro");

        var exposure = cmd[1].Eval(vars) > 0.5;
        var width = Math.Abs(cmd[2].Eval(vars));
        var height = Math.Abs(cmd[3].Eval(vars));
        var centerX = cmd[4].Eval(vars);
        var centerY = cmd[5].Eval(vars);
        var rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;

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

    /// <summary>
    /// Handles the outline command for aperture macro.
    /// </summary>
    private static void HandleOutline(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

        if (cmd.Count < 3)
            throw new ArgumentException("Invalid outline command in aperture macro");

        var exposure = cmd[1].Eval(vars) > 0.5;
        var nVertices = cmd[2].Eval(vars);
        var rotationIndex = 5 + 2 * nVertices;
        var rotation = cmd.Count > (5 + 2 * nVertices) ? cmd.Last().Eval(vars) : 0;

        if (nVertices < 3 || cmd.Count < rotationIndex || cmd.Count > rotationIndex + 1)
            throw new ArgumentException("Invalid outline command in aperture macro");

        var nVerticesInt = (int)Math.Round(nVertices);

        var paths = new PathsD(nVerticesInt);

        for (var i = 0; i < nVerticesInt; i++)
        {
            var x = fmt.ToFixed(cmd[3 + 2 * i].Eval(vars));
            var y = fmt.ToFixed(cmd[4 + 2 * i].Eval(vars));
            paths.Add(new PathD { new PointD(x, y) });
        }

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI, 1.0, true);
    }

    /// <summary>
    /// Handles the polygon command for aperture macro.
    /// Currently not implemented.
    /// </summary>
    private static void HandlePolygon(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

        if (cmd.Count is < 6 or > 7)
            throw new ArgumentException("Invalid polygon command in aperture macro");

        var exposure = cmd[1].Eval(vars) > 0.5;
        var nVertices = cmd[2].Eval(vars);
        var centerX = cmd[3].Eval(vars);
        var centerY = cmd[4].Eval(vars);
        var diameter = Math.Abs(cmd[5].Eval(vars));
        var rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;


        var nVerticesInt = (int)Math.Round(nVertices);

        var paths = new PathsD(nVerticesInt);

        for (var i = 0; i < nVerticesInt; i++)
        {
            var angle = (i / nVertices) * 2.0 * Math.PI;
            var x = centerX + diameter * 0.5 * Math.Cos(angle);
            var y = centerY + diameter * 0.5 * Math.Sin(angle);
            paths.Add(new PathD { new PointD(fmt.ToFixed(x), fmt.ToFixed(y)) });
        }

        aperture.DrawPaths(paths, exposure, 0, 0, false, false, rotation / 180 * Math.PI);
    }

    /// <summary>
    /// Handles the moire command for aperture macro.
    /// Currently not implemented.
    /// </summary>
    private static void HandleMoire(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {


    }

    /// <summary>
    /// Handles the thermal command for aperture macro.
    /// Currently not implemented.
    /// </summary>
    private static void HandleThermal(List<Expression> cmd, Dictionary<int, double> vars, ApertureBase aperture, ILayerFormat fmt)
    {

    }
}
