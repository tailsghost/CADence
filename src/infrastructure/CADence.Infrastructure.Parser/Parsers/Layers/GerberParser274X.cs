using CADence.Infrastructure.Parser.Abstractions;

namespace CADence.Infrastructure.Parser.Parsers;

public class GerberParser274X(string file) : IParser
{
    private string FILE { get; init; } = file;
}