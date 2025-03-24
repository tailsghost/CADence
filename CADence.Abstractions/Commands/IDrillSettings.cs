using CADence.Abstractions.Apertures;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Abstractions.Commands;

public interface IDrillSettings
{
    bool IsDone { get; set; }
    string cmd { get; set; }
    string fcmd { get; set; }
    public ILayerFormat format { get; set; }
    double MinHole { get; set; }
    public ParseState ParseState { get; set; }
    public RoutMode RoutMode { get; set; }
    public bool Plated { get; set; }
    public PointD LastPoint { get; set; }
    public PointD StartPoint { get; set; }
    public PointD CurrentPoint { get; set; }
    public PathD Points { get; set; }
    public Stack<ApertureBase> ApertureStack { get; set; }
    public (int integerDigits, int decimalDigits) FileFormat { get; set; }
    public ApertureBase Pth { get; set; }
    public ApertureBase Npth { get; set; }
    void CommitPath();
    void AddArc(PointD startPoint, PointD endPoint, double radius, bool ccw);
    ITool Tool { get; set; }
    Dictionary<int, ITool> Tools { get; set; }
}