namespace CADence.Abstractions.Commands;

/// <summary>
/// Generic interface for fabric commands to process settings of type T.
/// </summary>
public interface IFabricCommand<T>
{
    /// <summary>
    /// Adds a command function associated with a specific command key.
    /// </summary>
    void Add(string startCommand, Func<ICommand<T>> command);

    /// <summary>
    /// Removes a command function by its starting command key.
    /// </summary>
    void Remove(string startCommand);

    /// <summary>
    /// Executes the command based on the provided settings.
    /// </summary>
    public T ExecuteCommand(T settings);
}
