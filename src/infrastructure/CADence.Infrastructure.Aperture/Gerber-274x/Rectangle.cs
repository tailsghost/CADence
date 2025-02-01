using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Models.Format;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public sealed class Rectangle : ApertureBase
{
    /// <summary>
    /// Размер по оси X.
    /// </summary>
    private double XSize { get; set; }

    /// <summary>
    /// Размер по оси Y.
    /// </summary>
    private double YSize { get; set; }

    private readonly GeometryFactory _geomFactory = new GeometryFactory();

    /// <summary>
    /// Конструктор апертуры типа Rectangle.
    /// Параметры передаются в виде списка строк (csep) и формата ApertureFormat.
    /// Ожидается, что csep содержит от 3 до 4 элементов:
    /// csep[0]: идентификатор типа апертуры.
    /// csep[1]: размер по оси X (ширина прямоугольника).
    /// csep[2]: размер по оси Y (высота прямоугольника).
    /// csep[3] (опционально): диаметр отверстия (HoleDiameter). Если задан, отверстие добавляется внутрь апертуры.
    /// </summary>
    /// <param name="csep">Список строковых параметров апертуры.</param>
    /// <param name="format">Объект формата апертуры.</param>
    public Rectangle(List<string> csep, ApertureFormat format)
    {
        if (csep.Count is < 3 or > 4)
        {
            throw new ArgumentException("Invalid rectangle aperture");
        }
        
        XSize = Math.Abs(format.ParseFloat(csep[1]));
        YSize = Math.Abs(format.ParseFloat(csep[2]));
        
        HoleDiameter = csep.Count > 3 ? format.ParseFloat(csep[3]) : 0;
        
        var coords = new Coordinate[5];
        coords[0] = new Coordinate(XSize / 2.0, YSize / 2.0);
        coords[1] = new Coordinate(XSize / 2.0, -YSize / 2.0);
        coords[2] = new Coordinate(-XSize / 2.0, -YSize / 2.0);
        coords[3] = new Coordinate(-XSize / 2.0, YSize / 2.0);
        coords[4] = coords[0];
        
        var shell = _geomFactory.CreateLinearRing(coords);

        LinearRing[] holes = null;
        
        if (HoleDiameter > 0)
        {
            var holeGeometry = GetHole();
            if (holeGeometry is global::NetTopologySuite.Geometries.Polygon holePoly)
            {
                holes = new LinearRing[] { (LinearRing)holePoly.ExteriorRing };
            }
            else
            {
                throw new Exception("The resulting hole geometry is not a polygon.");
            }
        }
        
        AdditiveGeometry = _geomFactory.CreatePolygon(shell, holes);
    }

    /// <summary>
    /// Метод, определяющий, является ли апертура простым полигоном (без отверстия).
    /// Если отверстие задано (HoleDiameter > 0), то апертура не является простым полигоном.
    /// </summary>
    /// <param name="diameter">Выходной параметр: диаметр отверстия.</param>
    /// <returns>True, если апертура является простым кругом (без отверстия); иначе false.</returns>
    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }
}