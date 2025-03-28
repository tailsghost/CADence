using CADence.Abstractions.Apertures;
using CADence.Abstractions.Commands;
using CADence.App.Abstractions.Formats;
using CADence.App.Abstractions.Parsers;
using Clipper2Lib;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace CADence.Core.Parsers;

public class GerberParser274X : IGerberParser
{
    private IServiceProvider _provider;
    private IGerberSettings _settings { get; set; }
    private IFabricCommand<IGerberSettings> _fabric { get; set; }

    public GerberParser274X(IGerberSettings settings, IFabricCommand<IGerberSettings> fabric, IServiceProvider provider)
    {
        _settings = settings;
        _fabric = fabric;
        _provider = provider;
    }

    public IGerberParser Execute(string file)
    {
        using var stream = new StringReader(file);

        SetupSettings();

        var is_attrib = false;
        var ss = new StringBuilder();

        while (stream.Peek() != -1)
        {

            var c = (char)stream.Read();
            if (char.IsWhiteSpace(c))
            {
                continue;
            }
            else if (c == '%')
            {
                if (ss.Length > 0) throw new InvalidOperationException("attribute mid-command");
                if (is_attrib) _settings.AmBuilder = null;
                is_attrib = !is_attrib;
            }
            else if (c == '*')
            {
                if (ss.Length == 0) throw new InvalidOperationException("empty command");

                var cmd = ss.ToString();

                if (_settings.AmBuilder != null)
                {
                    _settings.AmBuilder.Append(cmd);
                    ss.Clear();
                    continue;
                }

                _settings.cmd = cmd;

                _settings = _fabric.ExecuteCommand(_settings);

                if (!_settings.IsDone)
                {
                    break;
                }

                ss.Clear();
            }
            else
            {
                ss.Append(c);
            }
        }

        if (is_attrib)
        {
            throw new InvalidOperationException("unterminated attribute");
        }

        if (_settings.IsDone)
        {
            throw new InvalidOperationException("unterminated gerber file");
        }

        if (_settings.ApertureStack.Count != 1)
        {
            throw new InvalidOperationException("unterminated block aperture");
        }

        if (_settings.RegionMode)
        {
            throw new InvalidOperationException("unterminated region block");
        }

        return this;

    }

    public PathsD GetResult(bool BoardOutLine = false)
    {

        if (BoardOutLine)
        {
            var outline = _settings.ApertureStack.Peek().GetAdditive();
            outline.RemoveAt(FindLargestAreaPath(outline));
            return outline;
        }

        return _settings.ApertureStack.Peek().GetAdditive();
    }

    private int FindLargestAreaPath(PathsD paths)
    {
        var maxArea = double.MinValue;
        var count = 0;

        for (var i = 0; i < paths.Count; i++)
        {
            var area = Clipper.Area(paths[i]);
            if (area > maxArea)
            {
                maxArea = area;
                count = i;
            }
        }

        return count;
    }
    private void SetupSettings()
    {
        _settings.format = _provider.GetRequiredService<ILayerFormat>();
        _settings.imode = InterpolationMode.UNDEFINED;
        _settings.qmode = QuadrantMode.UNDEFINED;
        _settings.Pos = new PointD(0, 0);
        _settings.Polarity = true;
        _settings.apMirrorX = false;
        _settings.apMirrorY = false;
        _settings.apRotate = 0.0;
        _settings.apScale = 1.0;
        _settings.RegionMode = false;
        _settings.OutlineConstructed = false;
    }
}
