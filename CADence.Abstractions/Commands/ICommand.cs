namespace CADence.Abstractions.Commands;

public interface ICommand<T>
{
    public T Execute(T value);
}
