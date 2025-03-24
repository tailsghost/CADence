using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADence.Abstractions.Readers;

public interface IReader
{
    /// <summary>
    /// Парсит архив (ZIP или RAR) и извлекает файлы, возвращая их содержимое в виде словаря.
    /// </summary>
    /// <param name="stream">Поток данных, содержащий архив.</param>
    /// <param name="fileName">Имя архива, включая расширение.</param>
    /// <returns>Интерфейс <see cref="IInputData"/>.</returns>
    IInputData ParseArchive(Stream stream, string fileName);
}
