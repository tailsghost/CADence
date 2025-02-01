using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.NetTopologySuite;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public sealed class Obround : ApertureBase
{
    private double XSize { get; set; }
    private double YSize { get; set; }

    private readonly GeometryFactory _geomFactory = new GeometryFactory();

    /// <summary>
    /// Конструктор апертуры типа Obround.
    /// Принимает список параметров и формат, используемый для парсинга.
    /// Если задано отверстие (HoleDiameter > 0), оно добавляется в апертуру.
    /// </summary>
    /// <param name="csep">Список строковых параметров апертуры.</param>
    /// <param name="fmt">Объект формата апертуры для парсинга параметров.</param>
    public Obround(List<string> csep, ApertureFormat fmt)
    {
        if (csep.Count is < 3 or > 4)
        {
            throw new ArgumentException("Invalid obround aperture");
        }

        XSize = Math.Abs(fmt.ParseFloat(csep[1]));
        YSize = Math.Abs(fmt.ParseFloat(csep[2]));
        
        HoleDiameter = csep.Count > 3 ? fmt.ParseFloat(csep[3]) : 0;
        
        var halfX = XSize / 2.0;
        var halfY = YSize / 2.0;
        
        var r = Math.Min(halfX, halfY);
        
        var rectHalfX = halfX - r;
        var rectHalfY = halfY - r;
        
        var coordinate1 = new Coordinate(-rectHalfX, -rectHalfY);
        var coordinate2 = new Coordinate(rectHalfX, rectHalfY);
        var centerLine = _geomFactory.CreateLineString([coordinate1, coordinate2]);
        
        var aperture = centerLine.Render(r * 2.0, false);
        
        if (HoleDiameter > 0)
        {
            var hole = GetHole();
            if (aperture is global::NetTopologySuite.Geometries.Polygon aperturePoly 
                && hole is global::NetTopologySuite.Geometries.Polygon holePoly)
            {
                aperture = _geomFactory.CreatePolygon(
                    (LinearRing)aperturePoly.ExteriorRing, 
                    new LinearRing[] { (LinearRing)holePoly.ExteriorRing }
                );
            }
        }
        
        Dark = aperture;
    }

    /// <summary>
    /// Метод возвращает информацию о том, является ли Obround простым (без отверстия).
    /// Obround с заданным отверстием не считается простым.
    /// </summary>
    /// <param name="diameter">Выходной параметр: диаметр отверстия.</param>
    /// <returns>Возвращает false, так как Obround обычно не является простым кругом.</returns>
    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }
}