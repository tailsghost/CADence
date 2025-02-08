using CADence.Infrastructure.Aperture.Abstractions;
using CADence.Infrastructure.Parser.Abstractions;

namespace CADence.Infrastructure.Parser.Settings;

/// <summary>
/// ��������� ��� ������� Drill 274X.
/// ��������� ������� ��������� Drill-�������.
/// </summary>
public class DrillParser274xSettings : DrillParserSettingsBase
{
    /// <summary>
    /// ������� ���������
    /// </summary>
    public ApertureBase Aperture;

    /// <summary>
    /// ������� �������
    /// </summary>
    public Dictionary<int, ApertureBase> Apertures = [];

    /// <summary>
    /// ������� ������ ��������.
    /// </summary>
    public ApertureMacroBase AmBuilder;
}