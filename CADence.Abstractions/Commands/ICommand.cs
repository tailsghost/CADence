namespace CADence.Abstractions.Commands;

/// <summary>
/// Generic interface for a command that processes settings of type T.
/// </summary>
public interface ICommand<T>
{
    /// <summary>
    /// Executes the command with the provided settings.
    /// </summary>
    T Execute(T value);
}
