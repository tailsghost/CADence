using CADence.App.Abstractions.Formats;

namespace CADence.Abstractions.Apertures;


/// <summary>
/// Абстрактный базовый класс для макросов апертуры.
/// Содержит коллекции команд и определяет методы для добавления команд и построения апертуры.
/// </summary>
public interface IApertureMacro
{
    /// <summary>
    /// Список команд апертуры.
    /// </summary>
    List<Expressions.Expression> Cmd { get; }

    /// <summary>
    /// Список списков команд апертуры.
    /// </summary>
    List<List<Expressions.Expression>> cmds { get; }

    /// <summary>
    /// Добавляет команду в макрос апертуры.
    /// </summary>
    void Append(string cmd);

    /// <summary>
    /// Строит объект апертуры на основе списка строковых команд и формата апертуры.
    /// </summary>
    /// <param name="csep">Список строковых команд.</param>
    /// <param name="format">Формат апертуры.</param>
    /// <returns>Построенный объект апертуры.</returns>
    public ApertureBase Build(List<string> csep, ILayerFormat format);
}
