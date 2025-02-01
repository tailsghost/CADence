using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.NetTopologySuite;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public sealed class Circle : ApertureBase
{
    private double Diameter { get; set; }
    
    private readonly GeometryFactory _geomFactory = new GeometryFactory();

    /// <summary>
    /// Конструктор апертуры типа Circle.
    /// Принимает список параметров и формат, используемый для парсинга.
    /// Если задано отверстие (HoleDiameter > 0), оно добавляется в апертуру.
    /// </summary>
    /// <param name="csep">Список строковых параметров апертуры.</param>
    /// <param name="format">Объект формата апертуры.</param>
    public Circle(List<string> csep, ApertureFormat format)
    {
        if (csep.Count < 2 || csep.Count > 3)
        {
            throw new ArgumentException("Invalid circle aperture");
        }
        
        Diameter = format.ParseFloat(csep[1]);
        HoleDiameter = csep.Count > 2 ? format.ParseFloat(csep[2]) : 0;


        var aperture = _geomFactory.CreatePoint(new Coordinate(0, 0)).Render(Diameter, false);
        
        if (HoleDiameter > 0)
        {
            var hole = GetHole();

            if (aperture is global::NetTopologySuite.Geometries.Polygon aperturePoly &&
                hole is global::NetTopologySuite.Geometries.Polygon holePoly)
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
    /// Метод возвращает информацию о том, является ли круг простым (без отверстия).
    /// Если отверстие задано (HoleDiameter > 0), круг не считается простым.
    /// </summary>
    /// <param name="diameter">Диаметр отверстия.</param>
    /// <returns>Возвращает true, если круг не имеет отверстия, иначе false.</returns>
    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = HoleDiameter;
        return !(HoleDiameter > 0);
    }
}