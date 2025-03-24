using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Abstractions.Apertures;

/// <summary>
/// Базовый класс для работы с апертурами, включающий операции рисования, трансформации и объединения геометрий.
/// </summary>
public class ApertureBase
{
    protected PathsD _accumulatedGeometries = new(500);

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
        AdditiveGeometry = new(750);
        SubtractiveGeometry = new(100);
    }

    public virtual ApertureBase Render(List<string> csep, ILayerFormat format)
    {
        return this;
    }


    /// <summary>
    /// Накопленная геометрия, хранящая временные изменения перед фиксацией.
    /// </summary>
    public PathsD? Accumulated { get; protected set; } = null;

    /// <summary>
    /// Текущая полярность (true - объединение, false - вычитание).
    /// </summary>
    private bool ACCUM_POLARITY { get; set; }

    /// <summary>
    /// Основная (добавляемая) геометрия.
    /// </summary>
    protected PathsD AdditiveGeometry { get; set; }

    /// <summary>
    /// Второстепенная (вычитаемая) геометрия.
    /// </summary>
    protected PathsD SubtractiveGeometry { get; set; }

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
    public void DrawPaths(PathsD geometry, bool polarity = true)
    {
        if (!geometry.Any())
            return;

        if (polarity != ACCUM_POLARITY)
            CommitPaths();

        ACCUM_POLARITY = polarity;

        for (var i = 0; i < geometry.Count; i++)
        {
            _accumulatedGeometries.Add(geometry[i]);
        }
    }

    /// <summary>
    /// Фиксирует накопленные геометрии, объединяя их с итоговыми.
    /// В зависимости от типа (AdditiveGeometry или SubtractiveGeometry) и полярности выполняется объединение или разность.
    /// </summary>
    protected void CommitPaths(CommitPathType type = CommitPathType.Additive)
    {
        if (!_accumulatedGeometries.Any())
            return;

        _accumulatedGeometries = Clipper.SimplifyPaths(_accumulatedGeometries, 1e-6);

        if (ACCUM_POLARITY)
        {
            if (type == CommitPathType.Additive)
                AdditiveGeometry = Clipper.Union(AdditiveGeometry, _accumulatedGeometries, FillRule.NonZero);
            else
                SubtractiveGeometry = Clipper.Difference(SubtractiveGeometry, _accumulatedGeometries, FillRule.NonZero);
        }
        else
        {
            if (type == CommitPathType.Additive)
                AdditiveGeometry = Clipper.Difference(AdditiveGeometry, _accumulatedGeometries, FillRule.NonZero);
            else
                SubtractiveGeometry = Clipper.Union(SubtractiveGeometry, _accumulatedGeometries, FillRule.NonZero);
        }

        Simplified = false;
        _accumulatedGeometries.Clear();
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
        PathsD geometry,
        bool polarity,
        double translateX,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0,
        bool specialFillType = false)
    {
        if (geometry.Count == 0) return;

        if (specialFillType) CommitPaths();

        DrawPaths(geometry, polarity);

        double ixx = mirrorX ? -scale : scale;
        double iyy = mirrorY ? -scale : scale;
        double sinRot = Math.Sin(rotate);
        double cosRot = Math.Cos(rotate);

        double xx = ixx * cosRot;
        double xy = ixx * sinRot;
        double yx = iyy * -sinRot;
        double yy = iyy * cosRot;

        double cx;
        double cy;

        PointD point;

        for (var j = 0; j < geometry.Count; j++)
        {
            var pathCopy = new PathD(geometry[j].Count);
            for (int i = 0; i < geometry[j].Count; i++)
            {
                point = geometry[j][i];
                cx = point.x * xx + point.x * yx;
                cy = point.x * xy + point.y * yy;
                pathCopy.Add(new PointD(cx + translateX,
                    cy + translateY));
            }

            if (_accumulatedGeometries.Count > 0)
            {
                _accumulatedGeometries[_accumulatedGeometries.Count - 1] = pathCopy;
            }
            else
            {
                _accumulatedGeometries.Add(pathCopy);
            }
        }

        if (mirrorX != mirrorY)
        {
            for (int i = _accumulatedGeometries.Count - geometry.Count; i < _accumulatedGeometries.Count; i++)
            {
                _accumulatedGeometries[i].Reverse();
            }
        }

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
    public PathsD? GetSubtractive()
    {
        CommitPaths(CommitPathType.Subtractive);
        Simplify(CommitPathType.Subtractive);
        return SubtractiveGeometry;
    }

    /// <summary>
    /// Фиксирует все накопленные объекты и выполняет упрощение основной геометрии.
    /// </summary>
    /// <returns>Упрощённая основная геометрия.</returns>
    public PathsD GetAdditive()
    {
        CommitPaths();
        Simplify();
        return AdditiveGeometry;
    }

    /// <summary>
    /// Упрощает геометрию
    /// </summary>
    /// <param name="type"></param>
    private void Simplify(CommitPathType type = CommitPathType.Additive)
    {
        if (Simplified) return;
        if (type == CommitPathType.Additive)
        {
            AdditiveGeometry = Clipper.Union(AdditiveGeometry, FillRule.EvenOdd);
            AdditiveGeometry = Clipper.SimplifyPaths(AdditiveGeometry, 1e-6);
        }
        else
        {
            SubtractiveGeometry = Clipper.Union(SubtractiveGeometry, FillRule.EvenOdd);
            SubtractiveGeometry = Clipper.SimplifyPaths(SubtractiveGeometry, 1e-6);
        }

        Simplified = true;
    }

    /// <summary>
    /// Виртуальный метод с возможностью переопределения базовой логики генерации отверстия.
    /// Если HoleDiameter меньше или равен 0, возвращается пустая коллекция геометрий.
    /// В противном случае генерируется круг, представляющий отверстие, с использованием буферизации точки.
    /// </summary>
    /// <returns> Возвращает геометрию отверстия.</returns>
    protected PathsD GetHole(ILayerFormat format)
    {

        if (HoleDiameter <= 0.0)
        {
            return [];
        }

        var holePath = new PathD
        {
            new PointD(0,0)
        };

        var paths = new PathsD { holePath }.Render(HoleDiameter,
            false, format.BuildClipperOffset());

        Clipper.ReversePaths(paths);

        return paths;
    }
}
