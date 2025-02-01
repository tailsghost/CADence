using CADence.Infrastructure.Aperture.NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Simplify;

namespace CADence.Infrastructure.Aperture.Abstractions;

public abstract class ApertureBase
{
    private readonly GeometryFactory _geomFactory = new GeometryFactory();
    protected double HoleDiameter;

    protected ApertureBase()
    {
        ACCUM_POLARITY = true;
        Simplified = false;
    }

    
    public Geometry? Accumulated { get; protected set; } = null;
    private bool ACCUM_POLARITY { get; set; }
    protected Geometry AdditiveGeometry { get; set; }
    protected Geometry SubtractiveGeometry { get; set; }
    protected bool Simplified { get; set; }


    /// <summary>
    /// Абстрактный метод проверки апертуры 
    /// </summary>
    /// <param name="diameter">Возвращает из апертуры диаметр.</param>
    /// <returns>Возвращает bool значение условия полярности диаметра апертуры</returns>
    public abstract bool IsSimpleCircle(out double diameter);

    /// <summary>
    /// Рисует (накапливает) геометрию – один или несколько контуров (в виде объекта Geometry).
    /// В зависимости от полярности происходит дальнейшее объединение или вычитание.
    /// </summary>
    /// <param name="geometry">Геометрия для рисования.</param>
    /// <param name="polarity">Полярность: true для объединения, false для вычитания.</param>
    public void DrawPaths(Geometry geometry, bool polarity = true)
    {
        if (geometry.IsEmpty) return;

        if (polarity != ACCUM_POLARITY)
            CommitPaths();

        ACCUM_POLARITY = polarity;

        Accumulated = Accumulated?.Union(geometry);
    }

    /// <summary>
    /// Фиксирует накопленные геометрии, объединяя их с итоговыми.
    /// В зависимости от типа (Dark или Clear) и полярности выполняется объединение или разность.
    /// </summary>
    protected void CommitPaths()
    {
        if (Accumulated == null || Accumulated.IsEmpty)
            return;

        if (ACCUM_POLARITY)
        {
            AdditiveGeometry = AdditiveGeometry.IsEmpty ? Accumulated : AdditiveGeometry.Union(Accumulated);
            SubtractiveGeometry = SubtractiveGeometry.IsEmpty ? _geomFactory.CreateGeometryCollection(null) : SubtractiveGeometry.Difference(Accumulated);
        }
        else
        {
            AdditiveGeometry = AdditiveGeometry.IsEmpty ? _geomFactory.CreateGeometryCollection(null) : AdditiveGeometry.Difference(Accumulated);
            SubtractiveGeometry = SubtractiveGeometry.IsEmpty ? Accumulated : SubtractiveGeometry.Union(Accumulated);
        }

        Accumulated = null;
        Simplified = false;
    }

    /// <summary>
    /// Рисует геометрию с применением аффинных преобразований: сдвиг, отражение, поворот, масштаб.
    /// </summary>
    /// <param name="geometry">Геометрия для преобразования и рисования.</param>
    /// <param name="polarity">Полярность для объединения или вычитания.</param>
    /// <param name="translateX">Смещение по оси X.</param>
    /// <param name="translateY">Смещение по оси Y.</param>
    /// <param name="mirrorX">Отражение по оси X.</param>
    /// <param name="mirrorY">Отражение по оси Y.</param>
    /// <param name="rotate">Угол поворота в радианах.</param>
    /// <param name="scale">Масштабирование.</param>
    /// <param name="specialFillType">Если true – фиксирует накопления до и после преобразований.</param>
    private void DrawPaths(
        Geometry geometry,
        bool polarity,
        double translateX,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0,
        bool specialFillType = false)
    {
        if (geometry.Coordinates.Length == 0) return;

        if (specialFillType) CommitPaths();

        DrawPaths(geometry, polarity);

        var scaleX = mirrorX ? -scale : scale;
        var scaleY = mirrorY ? -scale : scale;

        var scaleTransform = AffineTransformation.ScaleInstance(scaleX, scaleY);
        var rotateTransform = AffineTransformation.RotationInstance(rotate);
        var translateTransform = AffineTransformation.TranslationInstance(translateX, translateY);

        var transform = scaleTransform.Compose(rotateTransform).Compose(translateTransform);

        var transformed = transform.Transform(geometry);

        DrawPaths(transformed, polarity);

        if (specialFillType) CommitPaths();
    }

    /// <summary>
    /// Рисует апертуру с заданными параметрами преобразования.
    /// </summary>
    /// <param name="aperture">Апертура для рисования.</param>
    /// <param name="polarity">Полярность для объединения или вычитания.</param>
    /// <param name="translateX">Смещение по оси X.</param>
    /// <param name="translateY">Смещение по оси Y.</param>
    /// <param name="mirrorX">Отражение по оси X.</param>
    /// <param name="mirrorY">Отражение по оси Y.</param>
    /// <param name="rotate">Угол поворота в радианах.</param>
    /// <param name="scale">Масштабирование.</param>
    public void DrawAperture(
        ApertureBase aperture,
        bool polarity = true,
        double translateX = 0,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0)
    {
        DrawPaths(aperture.AdditiveGeometry, polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
        DrawPaths(aperture.AdditiveGeometry, !polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
    }

    /// <summary>
    /// Фиксирует все накопленные объекты и выполняет упрощение второстепенной геометрии.
    /// </summary>
    /// <returns>Упрощённая второстепенная геометрия.</returns>
    public Geometry? GetClear()
    {
        CommitPaths();
        return Simplify(SubtractiveGeometry);
    }

    /// <summary>
    /// Фиксирует все накопленные объекты и выполняет упрощение основной геометрии.
    /// </summary>
    /// <returns>Упрощённая основная геометрия.</returns>
    public Geometry? GetDark()
    {
        CommitPaths();
        return Simplify(AdditiveGeometry);
    }

    /// <summary>
    /// Упрощает переданную геометрию с сохранением топологии, используя допуск 1e-6.
    /// </summary>
    /// <param name="geometry">Геометрия для упрощения.</param>
    /// <returns>Упрощённая геометрия.</returns>
    private Geometry? Simplify(Geometry? geometry)
    {
        if (geometry == null || geometry.IsEmpty) return geometry;
        return TopologyPreservingSimplifier.Simplify(geometry, 1e-6);
    }

    /// <summary>
    /// Виртуальный метод с возможностью переопределения базовой логики генерации отверстия.
    /// Если HoleDiameter меньше или равен 0, возвращается пустая коллекция геометрий.
    /// В противном случае генерируется круг, представляющий отверстие, с использованием буферизации точки.
    /// </summary>
    /// <returns> Возвращает геометрию отверстия.</returns>
    protected virtual Geometry GetHole()
    {
        if (HoleDiameter <= 0.0)
        {
            return new GeometryCollection([], _geomFactory);
        }

        var point = _geomFactory.CreatePoint(new Coordinate(0, 0));

        var polygon = point.Render(HoleDiameter, false);

        return polygon.Reverse();
    }
}