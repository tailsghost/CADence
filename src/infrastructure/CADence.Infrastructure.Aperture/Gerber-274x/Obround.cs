using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.NetTopologySuite;
using CADence.Models.Format;
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
    /// Ожидается, что csep содержит от 3 до 4 элементов:
    /// - csep[0]: идентификатор типа апертуры.
    /// - csep[1]: размер по оси X (XSize).
    /// - csep[2]: размер по оси Y (YSize).
    /// - csep[3] (опционально): диаметр отверстия (HoleDiameter). Если задан, отверстие добавляется внутрь апертуры.
    /// </summary>
    /// <param name="csep">Список строковых параметров апертуры.</param>
    /// <param name="format">Объект формата апертуры.</param>
    public Obround(List<string> csep, ApertureFormat format)
    {
        if (csep.Count is < 3 or > 4)
        {
            throw new ArgumentException("Invalid obround aperture");
        }

        XSize = Math.Abs(format.ParseFloat(csep[1]));
        YSize = Math.Abs(format.ParseFloat(csep[2]));
        
        HoleDiameter = csep.Count > 3 ? format.ParseFloat(csep[3]) : 0;
        
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
        
        AdditiveGeometry = aperture;
    }

    /// <summary>
    /// Метод, определяющий, является ли апертура простым кругом (без отверстия).
    /// Если отверстие задано (HoleDiameter > 0), то апертура не является простым кругом.
    /// </summary>
    /// <param name="diameter">Выходной параметр: диаметр отверстия.</param>
    /// <returns>True, если апертура является простым кругом (без отверстия); иначе false.</returns>
    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }
}