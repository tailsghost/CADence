namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Абстрактный базовый класс для команд фабрики.
/// Управляет коллекцией команд и позволяет выполнять команды по ключу.
/// </summary>
/// <typeparam name="T">Тип параметров, с которыми работают команды.</typeparam>
public abstract class FabricCommandBase<T>
{
    /// <summary>
    /// Словарь команд, сопоставляющий строковый ключ с фабрикой команд.
    /// </summary>
    private readonly Dictionary<string, Func<CommandBase<T>>> _commands = new();

    /// <summary>
    /// Добавляет команду в коллекцию.
    /// </summary>
    /// <param name="startCommand">Ключ команды.</param>
    /// <param name="command">Функция, возвращающая экземпляр команды.</param>
    /// <exception cref="ArgumentException">Выбрасывается, если ключ пустой или функция команды равна null.</exception>
    public void Add(string startCommand, Func<CommandBase<T>> command)
    {
        if (string.IsNullOrWhiteSpace(startCommand) || command == null)
            throw new ArgumentException("Invalid command parameters.");

        _commands.TryAdd(startCommand, command);
    }
    /// <summary>
    /// Удаляет команду из коллекции по ключу.
    /// </summary>
    /// <param name="startCommand">Ключ команды для удаления.</param>
    public void Remove(string startCommand)
        => _commands.Remove(startCommand);
    
    /// <summary>
    /// Выполняет команду по заданному ключу с переданными настройками.
    /// </summary>
    /// <param name="startCommand">Ключ команды.</param>
    /// <param name="settings">Параметры, которые будут изменены командой.</param>
    /// <returns>Обновлённые параметры. Если команда не найдена, возвращаются исходные параметры.</returns>
    public T ExecuteCommand(string startCommand, T settings)
    {
        if (_commands.TryGetValue(startCommand, out var command))
        {
            return command().Execute(settings);
        }
        throw new ArgumentException("Command no found.");
    }
}