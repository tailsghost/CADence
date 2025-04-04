
using ExtensionClipper2.Core;

namespace CADence.App.Abstractions.Parsers;

/// <summary>
/// Interface for parsing drill commands.
/// </summary>
public interface IDrillParser
{
    /// <summary>
    /// Executes the drill parsing for a list of drill command strings.
    /// </summary>
    /// <param name="drills">List of drill commands.</param>
    /// <returns>An instance of IDrillParser with parsed drill geometry.</returns>
    IDrillParser Execute(List<string> drills);

    /// <summary>
    /// Retrieves the resulting drill geometry layer.
    /// </summary>
    /// <returns>The drill geometry as PathsD.</returns>
    PathsD GetLayer();
}
