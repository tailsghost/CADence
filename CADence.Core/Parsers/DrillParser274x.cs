using CADence.Abstractions.Commands;
using CADence.App.Abstractions.Parsers;
using CADence.Core.Apertures.Gerber_274;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.Core.Parsers;

public class DrillParser274X : IDrillParser
{
    private IServiceProvider _provider;

    private PathsD polygons = new();
    private IDrillSettings _settings;
    private IFabricCommand<IDrillSettings> _fabric { get; set; }
    public double MinHoleDiameter { get; private set; } = double.MaxValue;
    public PathsD _drillGeometry { get; set; } = new PathsD(600);

    public DrillParser274X(IServiceProvider provider, IFabricCommand<IDrillSettings> fabric)
    {
        _provider = provider;
        _fabric = fabric;
    }

    public IDrillParser Execute(List<string> drills)
    {
        foreach (string drill in drills)
        {
            using var stream = new StringReader(string.Join("\n", drill));
            string? line;

            _settings = _provider.GetRequiredService<IDrillSettings>();
            SetupSettings();

            while ((line = stream.ReadLine()) != null)
            {
                _settings.cmd = line;
                _settings = ExecuteCommand();
                if (!_settings.IsDone) break;
            }

            var geom = GetGeometryDrill();

            if (geom != null)
            {
                if (_drillGeometry.Count == 0)
                    _drillGeometry = geom;
                else
                    for (var i = 0; i < geom.Count; i++)
                        _drillGeometry.Add(geom[i]);
            }

            MinHoleDiameter = Math.Min(_settings.MinHole, MinHoleDiameter);
        }

        return this;

    }

    private PathsD GetGeometryDrill(bool plated = true, bool unplated = true)
    {
        PathsD result = new();

        if (plated)
        {
            if (unplated)
            {
                var platedGeom = _settings.Pth.GetAdditive();
                var unplatedGeom = _settings.Npth.GetAdditive();

                if (platedGeom != null && unplatedGeom != null)
                {
                    result = Clipper.Union(platedGeom, unplatedGeom, FillRule.Positive);
                }
                else if (platedGeom != null)
                {
                    result = platedGeom;
                }
                else
                {
                    result = unplatedGeom;
                }
            }
            else
            {
                result = _settings.Pth.GetAdditive();
            }
        }

        result?.Reverse();
        return result;
    }

    private IDrillSettings ExecuteCommand()
    {
        if (_settings.ParseState == ParseState.HEADER)
        {
            switch (_settings.cmd[0])
            {
                case ';':
                    _settings.cmd = _settings.cmd.Substring(1);
                    return _fabric.ExecuteCommand(_settings);
                case 'T':
                    _settings.fcmd = _settings.cmd;
                    _settings.cmd = "T";
                    return _fabric.ExecuteCommand(_settings);
                default:
                    return _fabric.ExecuteCommand(_settings);
            }
        }
        else if (_settings.ParseState == ParseState.BODY)
        {
            if (_settings.cmd[0] != ';')
            {
                switch (_settings.cmd[0])
                {
                    case 'X':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "X";
                        return _fabric.ExecuteCommand(_settings);
                    case 'Y':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "Y";
                        return _fabric.ExecuteCommand(_settings);
                    case 'T':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "T";
                        return _fabric.ExecuteCommand(_settings);
                    case 'G':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "G";
                        return _fabric.ExecuteCommand(_settings);
                    case 'M':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "M";
                        return _fabric.ExecuteCommand(_settings);
                    default:
                        throw new InvalidOperationException("unknown/unexpected command: " + _settings.cmd);
                }
            }
            _settings.IsDone = true;
            return _settings;
        }

        return _fabric.ExecuteCommand(_settings);
    }

    private void SetupSettings()
    {
        _settings.ParseState = ParseState.PRE_HEADER;
        _settings.Pth = _provider.GetRequiredService<Drill>();
        _settings.Npth = _provider.GetRequiredService<Drill>();
        _settings.format.ConfigureFormat(4, 3);
        _settings.format.ConfigureMillimeters();
        _settings.CurrentPoint = new PointD(0, 0);
        _settings.RoutMode = RoutMode.DRILL;
    }

    public PathsD GetLayer()
    {
        return _drillGeometry;
    }
}
