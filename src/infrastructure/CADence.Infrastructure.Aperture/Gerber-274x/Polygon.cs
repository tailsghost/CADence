using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Models.Format;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public sealed class Polygon : ApertureBase
{
    private readonly GeometryFactory _geomFactory = new GeometryFactory();
    private double Diameter { get; set; }
    private int NVertices { get; set; }
    private double Rotation { get; set; }

    /// <summary>
    /// Конструктор апертуры типа "Полигон".
    /// Параметры передаются в виде списка строк (csep) и формата ConcreteFormat.
    /// Ожидается, что csep содержит от 3 до 5 элементов:
    /// csep[0]: идентификатор типа апертуры.
    /// csep[1] - диаметр полигона,
    /// csep[2] - количество вершин,
    /// csep[3] (опционально) - угол поворота в градусах,
    /// csep[4] (опционально) - диаметр отверстия (HoleDiameter). Если задан, отверстие добавляется внутрь апертуры.
    /// </summary>
    /// <param name="csep">Список строковых параметров апертуры.</param>
    /// <param name="fmt">Формат апертуры с методами для парсинга параметров.</param>
    public Polygon(List<string> csep, ApertureFormat fmt)
    {
        if (csep.Count < 3 || csep.Count > 5)
        {
            throw new ArgumentException("Invalid polygon aperture");
        }

        Diameter = fmt.ParseFloat(csep[1]);
        
        NVertices = int.Parse(csep[2]);
        if (NVertices < 3)
        {
            throw new ArgumentException("Invalid polygon aperture: число вершин должно быть не меньше 3");
        }
        
        Rotation = csep.Count > 3 ? double.Parse(csep[3]) * Math.PI / 180.0 : 0.0;
        
        HoleDiameter = csep.Count > 4 ? fmt.ParseFloat(csep[4]) : 0;
        
        var coords = new Coordinate[NVertices + 1];
        for (var i = 0; i < NVertices; i++)
        {
            var angle = ((double)i / NVertices) * 2.0 * Math.PI + Rotation;
            var x = Diameter * 0.5 * Math.Cos(angle);
            var y = Diameter * 0.5 * Math.Sin(angle);
            coords[i] = new Coordinate(x, y);
        }
        
        coords[NVertices] = coords[0];
        
        var shell = _geomFactory.CreateLinearRing(coords);

        LinearRing[]? holes = null;
        
        if (HoleDiameter > 0)
        {
            Geometry holeGeometry = GetHole();
            if (holeGeometry is global::NetTopologySuite.Geometries.Polygon holePoly)
            {
                holes = [(LinearRing)holePoly.ExteriorRing];
            }
            else
            {
                throw new Exception("Полученная геометрия отверстия не является полигоном.");
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
