using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Aperture.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Aperture.NetTopologySuite;
using CADence.Infrastructure.Command.Helpers;

namespace CADence.Infrastructure.Parser.Settings;

/// <summary>
/// Настройки для парсера Gerber 274X.
/// Наследует базовые параметры Gerber-парсера.
/// </summary>
public class GerberParser274xSettings : GerberParserSettingsBase
{

    GeometryFactory _geometryFactory = new GeometryFactory();

    /// <summary>
    /// Текущая аперткура
    /// </summary>
    public ApertureBase Aperture;

    /// <summary>
    /// Словарь апертур
    /// </summary>
    public Dictionary<int, ApertureBase> Apertures = [];
    /// <summary>
    /// Текущий макрос апертуры.
    /// </summary>
    public ApertureMacroBase AmBuilder;

    /// <summary>
    /// Минимальная толщина дорожки
    /// </summary>
    public double MinimumDiameter = double.MaxValue;

    public List<Coordinate> RegionAccum { get; } = new(1000);

    public void CommitRegion()
    {
        if (RegionAccum.Count < 3)
            return;

        Polygon region = _geometryFactory.CreatePolygon(RegionAccum.ToArray());

        if (region.Area < 0)
        {
            RegionAccum.Reverse();
            region = _geometryFactory.CreatePolygon(RegionAccum.ToArray());
        }

        ApertureStack.Peek().DrawPaths(region, Polarity);

        RegionAccum.Clear();
    }

    public void Interpolate(Point dest, Point center)
    {
        List<Coordinate> coordinates = new List<Coordinate>();

        if (imode == InterpolationMode.UNDEFINED)
        {
            throw new InvalidOperationException("Interpolate command before mode set");
        }
        else if (imode == InterpolationMode.LINEAR)
        {
            coordinates.Add(new Coordinate(Pos.X, Pos.Y));
            coordinates.Add(new Coordinate(dest.X, dest.Y));
        }
        else
        {
            CircularInterpolationHelper helper = null;
            bool ccw = imode == InterpolationMode.CIRCULAR_CCW;

            if (qmode == QuadrantMode.UNDEFINED)
            {
                throw new InvalidOperationException("Arc command before quadrant mode set");
            }
            else if (qmode == QuadrantMode.MULTI)
            {
                var arcCenter = new Point(Pos.X + center.X, Pos.Y + center.Y);
                Point ntsStart = new Point(Pos.X, Pos.Y);
                Point ntsEnd = new Point(dest.X, dest.Y);
                Point ntsCenter = new Point(arcCenter.X, arcCenter.Y);
                helper = new CircularInterpolationHelper(ntsStart, ntsEnd, ntsCenter, ccw, true);
            }
            else
            {
                for (int k = 0; k < 4; k++)
                {
                    var candidateCenter = new Point(
                        Pos.X + ((k & 1) == 1 ? center.X : -center.X),
                        Pos.Y + ((k & 2) == 2 ? center.Y : -center.Y)
                    );
                    Point ntsStart = new Point(Pos.X, Pos.Y);
                    Point ntsEnd = new Point(dest.X, dest.Y);
                    Point ntsCandidateCenter = new Point(candidateCenter.X, candidateCenter.Y);
                    var h2 = new CircularInterpolationHelper(ntsStart, ntsEnd, ntsCandidateCenter, ccw, false);
                    if (h2.IsSingleQuadrant())
                    {
                        if (helper == null || helper.Error() > h2.Error())
                            helper = h2;
                    }
                }
            }

            if (helper == null)
            {
                throw new InvalidOperationException("Failed to make circular interpolation");
            }

            coordinates = helper.ToCoordinates();
        }

        if (RegionMode)
        {
            RegionAccum.AddRange(coordinates.Skip(1));
            return;
        }

        if (Aperture == null)
        {
            throw new InvalidOperationException("Interpolate command before aperture set");
        }

        if (!Aperture.IsSimpleCircle(out double diameter))
        {
            throw new InvalidOperationException("Only simple circle apertures without a hole are supported for interpolation");
        }

        double thickness = diameter * apScale;
        if (thickness == 0)
            return;

        LineString path = new LineString(coordinates.ToArray());

        Geometry bufferedGeometry = path.Render(thickness, false);

        ApertureStack.Peek().DrawPaths(bufferedGeometry, Polarity);
    }

    public void DrawAperture()
    {
        if (Aperture == null)
        {
            throw new InvalidOperationException("Flash command before aperture set");
        }
        ApertureStack.Peek().DrawAperture(
            Aperture, Polarity, Pos.X, Pos.Y, apMirrorX, apMirrorY, apRotate, apScale);
    }
}