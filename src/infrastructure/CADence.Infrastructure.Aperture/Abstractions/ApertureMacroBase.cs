using CADence.Models.Format.Abstractions;
using System.Collections.Generic;

namespace CADence.Infrastructure.Aperture.Abstractions;

/// <summary>
/// Абстрактный базовый класс для макросов апертуры.
/// Содержит коллекции команд и определяет методы для добавления команд и построения апертуры.
/// </summary>
public abstract class ApertureMacroBase
{
    /// <summary>
    /// Список команд апертуры.
    /// </summary>
    public List<CADence.Aperture.Expression.Expression> Cmd = [];
    
    /// <summary>
    /// Список списков команд апертуры.
    /// </summary>
    public List<List<CADence.Aperture.Expression.Expression>> cmds { get; set; } = [];
    
    /// <summary>
    /// Добавляет команду в макрос апертуры.
    /// </summary>
    public abstract void Append(string cmd);
    
    /// <summary>
    /// Строит объект апертуры на основе списка строковых команд и формата апертуры.
    /// </summary>
    /// <param name="csep">Список строковых команд.</param>
    /// <param name="format">Формат апертуры.</param>
    /// <returns>Построенный объект апертуры.</returns>
    public abstract ApertureBase Build(List<string> csep, LayerFormatBase format);
}
