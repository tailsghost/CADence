using CADence.Infrastructure.Parser.Abstractions;

namespace CADence.Infrastructure.Parser.Parsers.Drills;

public class GerberParserDrill274X : IParser
{
    private List<string> DRILLS = new();

    public GerberParserDrill274X(List<string> drills)
    {
        DRILLS = drills;
    }
}