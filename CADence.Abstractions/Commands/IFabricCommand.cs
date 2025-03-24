namespace CADence.Abstractions.Commands;

public interface IFabricCommand<T>
{
    void Add(string startCommand, Func<ICommand<T>> command);
    void Remove(string startCommand);
    public T ExecuteCommand(T settings);
}
