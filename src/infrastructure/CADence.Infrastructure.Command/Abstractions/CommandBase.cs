namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Абстрактный базовый класс для команд с параметромизированным типом параметров.
/// </summary>
/// <typeparam name="T">Тип параметров, с которыми работает команда.</typeparam>
public abstract class CommandBase<T>
{
    /// <summary>
    /// Выполняет команду, изменяя переданные параметры.
    /// </summary>
    /// <param name="settings">Параметры для обработки.</param>
    /// <returns>Обновлённые параметры после выполнения команды.</returns>
    public abstract T Execute(T settings);
}