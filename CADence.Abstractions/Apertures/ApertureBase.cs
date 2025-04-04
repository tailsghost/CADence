using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.Abstractions.Apertures;

/// <summary>
/// Base class for aperture operations, including drawing, transforming, and merging geometries.
/// </summary>
public class ApertureBase
{
    protected PathsD _accumulatedGeometries = new(500);

    protected double HoleDiameter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApertureBase"/> class.
    /// Sets the initial polarity and simplification state.
    /// </summary>
    public ApertureBase()
    {
        ACCUM_POLARITY = true;
        Simplified = false;
        AdditiveGeometry = new(750);
        SubtractiveGeometry = new(100);
    }

    /// <summary>
    /// Renders the aperture based on provided parameters.
    /// </summary>
    /// <param name="csep">A list of strings representing aperture parameters.</param>
    /// <param name="format">The layer format to be used.</param>
    /// <returns>The rendered <see cref="ApertureBase"/> instance.</returns>
    public virtual ApertureBase Render(List<string> csep, ILayerFormat format)
    {
        return this;
    }


    /// <summary>
    /// Gets or sets the current polarity (true for union, false for subtraction).
    /// </summary>
    private bool ACCUM_POLARITY { get; set; }

    /// <summary>
    /// Gets or sets the primary (additive) geometry.
    /// </summary>
    protected PathsD AdditiveGeometry { get; set; }


    /// <summary>
    /// Gets or sets the secondary (subtractive) geometry.
    /// </summary>
    protected PathsD SubtractiveGeometry { get; set; }

    /// <summary>
    /// Gets a value indicating whether the geometry has been simplified.
    /// </summary>
    protected bool Simplified { get; set; }


    /// <summary>
    /// Checks if the aperture is a simple circle.
    /// </summary>
    /// <param name="diameter">Outputs the diameter of the aperture if it is a simple circle.</param>
    /// <returns><c>true</c> if the aperture is a simple circle; otherwise, <c>false</c>.</returns>
    public virtual bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }

    /// <summary>
    /// Draws (accumulates) the specified geometry (one or more contours) based on the polarity.
    /// </summary>
    /// <param name="geometry">The geometry to draw.</param>
    /// <param name="polarity">If <c>true</c> the geometry is added (union); if <c>false</c>, it is subtracted.</param>
    public void DrawPaths(PathsD geometry, bool polarity = true)
    {
        if (geometry.Count == 0)
            return;

        if (polarity != ACCUM_POLARITY)
            CommitPaths();

        ACCUM_POLARITY = polarity;

        for (var i = 0; i < geometry.Count; i++)
        {
            _accumulatedGeometries.Add(new PathD(geometry[i]));
        }
    }

    /// <summary>
    /// Commits the accumulated geometries by merging them into the final geometries.
    /// Depending on the type (additive or subtractive) and polarity, it performs union or difference operations.
    /// </summary>
    /// <param name="type">The commit type: additive or subtractive.</param>
    protected void CommitPaths(CommitPathType type = CommitPathType.Additive)
    {
        if (_accumulatedGeometries.Count == 0)
            return;

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
    /// Draws the specified geometry with affine transformations (translation, reflection, rotation, scaling).
    /// </summary>
    /// <param name="geometry">The geometry to transform and draw.</param>
    /// <param name="polarity">If <c>true</c> the geometry is added; if <c>false</c> it is subtracted.</param>
    /// <param name="translateX">Translation along the X axis.</param>
    /// <param name="translateY">Translation along the Y axis.</param>
    /// <param name="mirrorX">If set to <c>true</c>, mirrors the geometry along the X axis.</param>
    /// <param name="mirrorY">If set to <c>true</c>, mirrors the geometry along the Y axis.</param>
    /// <param name="rotate">Rotation angle in radians.</param>
    /// <param name="scale">Scaling factor.</param>
    /// <param name="specialFillType">If <c>true</c>, commits the accumulated geometries before and after transformation.</param>
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

        var ixx = mirrorX ? -scale : scale;
        var iyy = mirrorY ? -scale : scale;
        var sinRot = Math.Sin(rotate);
        var cosRot = Math.Cos(rotate);

        var xx = ixx * cosRot;
        var xy = ixx * sinRot;
        var yx = iyy * -sinRot;
        var yy = iyy * cosRot;

        for (var j = 0; j < geometry.Count; j++)
        {
            var pathCopy = new PathD(geometry[j].Count);
            for (var i = 0; i < geometry[j].Count; i++)
            {
                var point = geometry[j][i];
                var cx = point.X * xx + point.Y * yx;
                var cy = point.X * xy + point.Y * yy;
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
            for (var i = _accumulatedGeometries.Count - geometry.Count; i < _accumulatedGeometries.Count; i++)
            {
                _accumulatedGeometries[i].Reverse();
            }
        }

        if (specialFillType) CommitPaths();
    }

    /// <summary>
    /// Draws an aperture with the specified transformation parameters.
    /// </summary>
    /// <param name="aperture">The aperture to draw.</param>
    /// <param name="polarity">If <c>true</c> the aperture is added; if <c>false</c>, it is subtracted.</param>
    /// <param name="translateX">Translation along the X axis.</param>
    /// <param name="translateY">Translation along the Y axis.</param>
    /// <param name="mirrorX">If set to <c>true</c>, mirrors the aperture along the X axis.</param>
    /// <param name="mirrorY">If set to <c>true</c>, mirrors the aperture along the Y axis.</param>
    /// <param name="rotate">Rotation angle in radians.</param>
    /// <param name="scale">Scaling factor.</param>
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
    /// Commits all accumulated geometries and simplifies the subtractive geometry.
    /// </summary>
    /// <returns>The simplified subtractive geometry.</returns>
    public PathsD? GetSubtractive()
    {
        CommitPaths(CommitPathType.Subtractive);
        Simplify(CommitPathType.Subtractive);
        return SubtractiveGeometry;
    }

    /// <summary>
    /// Commits all accumulated geometries and simplifies the additive geometry.
    /// </summary>
    /// <returns>The simplified additive geometry.</returns>
    public PathsD GetAdditive()
    {
        CommitPaths();
        Simplify();
        return AdditiveGeometry;
    }

    /// <summary>
    /// Simplifies the geometry.
    /// </summary>
    /// <param name="type">Specifies whether to simplify additive or subtractive geometry.</param>
    private void Simplify(CommitPathType type = CommitPathType.Additive)
    {
        if (Simplified) return;
        if (type == CommitPathType.Additive)
        {
            AdditiveGeometry = Clipper.Union(AdditiveGeometry, FillRule.EvenOdd);
            //AdditiveGeometry = Clipper.SimplifyPaths(AdditiveGeometry, 1e-10);
        }
        else 
        {
            SubtractiveGeometry = Clipper.Union(SubtractiveGeometry, FillRule.EvenOdd);
            //SubtractiveGeometry = Clipper.SimplifyPaths(SubtractiveGeometry, 1e-10);
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
            return new();
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
