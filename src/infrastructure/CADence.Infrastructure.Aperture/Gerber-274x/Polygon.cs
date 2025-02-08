using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Models.Format;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public sealed class Polygon : ApertureBase
{
    private readonly GeometryFactory _geomFactory = new GeometryFactory();
    
    /// <summary>
    /// Диаметр полигона, определяющий расстояние между противоположными вершинами.
    /// </summary>
    private double Diameter { get; set; }

    /// <summary>
    /// Количество вершин полигона. 
    /// Должно быть не меньше 3 для формирования правильной геометрической фигуры.
    /// </summary>
    private int NVertices { get; set; }

    /// <summary>
    /// Угол поворота полигона в градусах, преобразованный в радианы.
    /// Определяет начальный угол смещения вершин относительно оси координат.
    /// </summary>
    private double Rotation { get; set; }

    /// <summary>
    /// Конструктор апертуры типа Polygon.
    /// Параметры передаются в виде списка строк (csep) и формата ApertureFormat.
    /// Ожидается, что csep содержит от 3 до 5 элементов:
    /// csep[0]: идентификатор типа апертуры.
    /// csep[1] - диаметр полигона,
    /// csep[2] - количество вершин,
    /// csep[3] (опционально) - угол поворота в градусах,
    /// csep[4] (опционально) - диаметр отверстия (HoleDiameter). Если задан, отверстие добавляется внутрь апертуры.
    ///
    /// Возможные ошибки:
    /// 1. <see cref="ArgumentException"/> – выбрасывается, если количество параметров в <see cref="csep"/> меньше 3 или больше 5.
    /// 2. <see cref="Exception"/> – выбрасывается, если результирующая геометрия отверстия не является полигоном.
    /// </summary>
    /// <param name="csep">Список строковых параметров апертуры.</param>
    /// <param name="format">Объект формата апертуры.</param>
    public Polygon(List<string> csep, LayerFormat format)
    {
        if (csep.Count is < 3 or > 5)
        {
            throw new ArgumentException("Invalid polygon aperture");
        }

        Diameter = format.ParseFloat(csep[1]);
        
        NVertices = int.Parse(csep[2]);
        if (NVertices < 3)
        {
            throw new ArgumentException("Invalid polygon aperture: число вершин должно быть не меньше 3");
        }
        
        Rotation = csep.Count > 3 ? double.Parse(csep[3]) * Math.PI / 180.0 : 0.0;
        
        HoleDiameter = csep.Count > 4 ? format.ParseFloat(csep[4]) : 0;
        
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
            var holeGeometry = GetHole();
            if (holeGeometry is global::NetTopologySuite.Geometries.Polygon holePoly)
            {
                holes = [(LinearRing)holePoly.ExteriorRing];
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
