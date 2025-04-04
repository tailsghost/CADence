using CADence.App.Abstractions.Formats;

namespace CADence.Abstractions.Apertures;


/// <summary>
/// Interface for an aperture macro.
/// Contains collections of commands and methods to append commands and build an aperture.
/// </summary>
public interface IApertureMacro
{
    /// <summary>
    /// List of aperture macro expressions (commands).
    /// </summary>
    List<Expressions.Expression> Cmd { get; }

    /// <summary>
    /// List of lists of aperture macro expressions.
    /// </summary>
    List<List<Expressions.Expression>> cmds { get; }

    /// <summary>
    /// Appends a command string to the macro.
    /// </summary>
    /// <param name="cmd">The command string.</param>
    void Append(string cmd);

    /// <summary>
    /// Builds an aperture object based on the list of string commands and the provided format.
    /// </summary>
    /// <param name="csep">The list of command strings.</param>
    /// <param name="format">The aperture format.</param>
    /// <returns>The constructed aperture.</returns>
    public ApertureBase Build(List<string> csep, ILayerFormat format);
}
