using CADence.Infrastructure.Parser.Abstractions;

namespace CADence.Infrastructure.Parser.Parsers.Drills;

public class GerberParserDrill274X : DrillParserBase
{
    private List<string> DRILLS = new();

    public GerberParserDrill274X(List<string> drills)
    {
        DRILLS = drills;
    }

    public override void Execute()
    {
        throw new NotImplementedException();
    }
}