using CADence.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.Abstractions;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

/// <summary>
/// Апертура созданная с помощью <see cref="ApertureMacroBase"/>
/// </summary>
public sealed class Unknown : ApertureBase
{
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
