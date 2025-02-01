using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.NetTopologySuite;
using CADence.Models.Format;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public sealed class Circle : ApertureBase
{
    /// <summary>
    /// Диаметр круга.
    /// </summary>
    private double Diameter { get; set; }
    
    private readonly GeometryFactory _geomFactory = new GeometryFactory();

    /// <summary>
    /// Конструктор апертуры типа Circle.
    /// Параметры передаются в виде списка строк (csep) и формата ApertureFormat.
    /// Если задано отверстие (HoleDiameter > 0), оно добавляется в апертуру.
    /// Ожидается, что csep содержит от 2 до 3 элементов:
    /// csep[0] – идентификатор типа апертуры
    /// csep[1] – диаметр круга (Diameter). Определяет размер апертуры.
    /// csep[2] – (опционально) диаметр отверстия (HoleDiameter). Если задан, отверстие добавляется внутрь апертуры.
    /// 
    /// Возможные ошибки:
    /// 1. <see cref="ArgumentException"/> – выбрасывается, если количество параметров в <see cref="csep"/> меньше 2 или больше 3.
    /// 2. <see cref="Exception"/> – выбрасывается, если результирующая геометрия отверстия не является полигоном.
    /// </summary>
    /// <param name="csep">Список строковых параметров апертуры.</param>
    /// <param name="format">Объект формата апертуры.</param>
    public Circle(List<string> csep, ApertureFormat format)
    {
        if (csep.Count is < 2 or > 3)
        {
            throw new ArgumentException("Invalid circle aperture");
        }
        
        Diameter = format.ParseFloat(csep[1]);
        HoleDiameter = csep.Count > 2 ? format.ParseFloat(csep[2]) : 0;


        var aperture = _geomFactory.CreatePoint(new Coordinate(0, 0)).Render(Diameter, false);
        
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
            else
            {
                throw new Exception("The resulting hole geometry is not a polygon.");
            }
        }

        AdditiveGeometry = aperture;
    }

    /// <summary>
    /// Метод, определяющий, является ли апертура простым кругом (без отверстия).
    /// Если отверстие задано (HoleDiameter > 0), то апертура не является простым кругом.
    /// </summary>
    /// <param name="diameter">Диаметр отверстия.</param>
    /// <returns>True, если апертура является простым кругом (без отверстия); иначе false.</returns>
    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = HoleDiameter;
        return !(HoleDiameter > 0);
    }
}