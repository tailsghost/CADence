using CADence.Infrastructure.Aperture.NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Operation.Union;

namespace CADence.Infrastructure.Aperture.Abstractions;

/// <summary>
/// Базовый класс для работы с апертурами, включающий операции рисования, трансформации и объединения геометрий.
/// </summary>
public class ApertureBase
{
    protected readonly List<Geometry> _accumulatedGeometries = new List<Geometry>(500);

    private readonly GeometryFactory _geomFactory;
    
    /// <summary>
    /// Диаметр отверстия, используемый при генерации отверстий в апертуре.
    /// </summary>
    protected double HoleDiameter;

    /// <summary>
    /// Конструктор базового класса апертуры.
    /// Инициализирует параметры полярности и состояния упрощения.
    /// </summary>
    public ApertureBase()
    {
        ACCUM_POLARITY = true;
        Simplified = false;
        _geomFactory = new GeometryFactory();
        AdditiveGeometry = _geomFactory.CreateGeometryCollection(new Geometry[0]);
        SubtractiveGeometry = _geomFactory.CreateGeometryCollection(new Geometry[0]);
    }

    
    /// <summary>
    /// Накопленная геометрия, хранящая временные изменения перед фиксацией.
    /// </summary>
    public Geometry? Accumulated { get; protected set; } = null;
    
    /// <summary>
    /// Текущая полярность (true - объединение, false - вычитание).
    /// </summary>
    private bool ACCUM_POLARITY { get; set; }
    
    /// <summary>
    /// Основная (добавляемая) геометрия.
    /// </summary>
    protected Geometry AdditiveGeometry { get; set; }
    
    /// <summary>
    /// Второстепенная (вычитаемая) геометрия.
    /// </summary>
    protected Geometry SubtractiveGeometry { get; set; }
    
    /// <summary>
    /// Флаг, указывающий, была ли геометрия упрощена.
    /// </summary>
    protected bool Simplified { get; set; }


    /// <summary>
    /// Абстрактный метод проверки апертуры 
    /// </summary>
    /// <param name="diameter">Возвращает из апертуры диаметр.</param>
    /// <returns>Возвращает bool значение условия полярности диаметра апертуры</returns>
    public virtual bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }

    /// <summary>
    /// Рисует (накапливает) геометрию – один или несколько контуров (в виде объекта Geometry).
    /// В зависимости от полярности происходит дальнейшее объединение или вычитание.
    /// </summary>
    /// <param name="geometry">Геометрия для рисования.</param>
    /// <param name="polarity">Полярность: true для объединения, false для вычитания.</param>
    public void DrawPaths(Geometry geometry, bool polarity = true)
    {
        if (geometry.IsEmpty)
            return;

        if (polarity != ACCUM_POLARITY)
            CommitPaths();

        ACCUM_POLARITY = polarity;

        _accumulatedGeometries.Add(geometry);
    }

    /// <summary>
    /// Фиксирует накопленные геометрии, объединяя их с итоговыми.
    /// В зависимости от типа (AdditiveGeometry или SubtractiveGeometry) и полярности выполняется объединение или разность.
    /// </summary>
    protected void CommitPaths()
    {
        if (_accumulatedGeometries.Count == 0)
            return;

        Geometry mergedAccumulated;
        if (_accumulatedGeometries.Count == 1)
        {
            mergedAccumulated = _accumulatedGeometries[0];
        }
        else
        {

            //var overlayParams = OverlayNG.Overlay();
            mergedAccumulated = UnaryUnionOp.Union(_accumulatedGeometries);
            // На этом месте, если геометрия сложная, то все ломается.
        }

        if (!mergedAccumulated.IsValid)
        {
            mergedAccumulated = mergedAccumulated.Buffer(0);
        }

        if (ACCUM_POLARITY)
        {
            AdditiveGeometry = OverlayNG.Overlay(AdditiveGeometry, mergedAccumulated, SpatialFunction.Union);
            SubtractiveGeometry = OverlayNG.Overlay(SubtractiveGeometry, mergedAccumulated, SpatialFunction.Difference);
        }
        else
        {
            AdditiveGeometry = OverlayNG.Overlay(AdditiveGeometry, mergedAccumulated, SpatialFunction.Difference);

            SubtractiveGeometry = OverlayNG.Overlay(SubtractiveGeometry, mergedAccumulated, SpatialFunction.Union);
        }

        _accumulatedGeometries.Clear();

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
    public void DrawPaths(
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
        DrawPaths(aperture.GetAdditive(), polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
        DrawPaths(aperture.GetSubtractive(), !polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
    }

    /// <summary>
    /// Фиксирует все накопленные объекты и выполняет упрощение второстепенной геометрии.
    /// </summary>
    /// <returns>Упрощённая второстепенная геометрия.</returns>
    public Geometry? GetSubtractive()
    {
        CommitPaths();
        return SimplifyGeometry(SubtractiveGeometry);
    }

    /// <summary>
    /// Фиксирует все накопленные объекты и выполняет упрощение основной геометрии.
    /// </summary>
    /// <returns>Упрощённая основная геометрия.</returns>
    public Geometry? GetAdditive()
    {
        CommitPaths();
        return SimplifyGeometry(AdditiveGeometry);
    }


    public Geometry SimplifyGeometry(Geometry geometry, double tolerance = 1e-8)
    {
        if (Simplified) return geometry;

        return geometry;
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

        var point = new Point(0, 0);

        var polygon = point.Render(HoleDiameter, false);

        return polygon.Reverse();
    }
}