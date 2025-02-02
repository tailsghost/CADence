using SharpCompress.Archives.Rar;
using System.IO.Compression;
using System.Text;
using CADence.Infrastructure.LayerFabric.Common.Abstractions;

namespace CADence.Infrastructure.LayerFabric.Readers;

public class BoardFileReader : IReader
{
    /// <summary>
    /// Неподдерживаемый формат файла в архиве.
    /// </summary>
    private readonly IEnumerable<string> _unsupported = new List<string>()
        { "config", "exe", "dll", "png", "zip", "gif", "jpeg", "doc", "docx", "jpg", "bmp", "svg" };

    private readonly InputData _inputData = new();

    public IInputData ParseArchive(Stream stream, string fileName)
    {
        Dictionary<string, string> data = new();

        if (!fileName.ToLower().EndsWith(".zip") && !fileName.ToLower().EndsWith(".rar"))
        {
            throw new ArgumentException("Unsupported file format. Only ZIP and RAR are supported.");
        }

        if (fileName.ToLower().EndsWith(".zip"))
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            for (var i = 0; i < archive.Entries.Count; i++)
            {
                var entry = archive.Entries[i];
                var result = ProcessArchiveEntry(entry.FullName, entry.Name, entry.Open);
                if (result != string.Empty)
                    data[entry.FullName] = result;
            }
        }
        else if (fileName.ToLower().EndsWith(".rar"))
        {
            using var archive = RarArchive.Open(stream);
            var entriesArray = new RarArchiveEntry[archive.Entries.Count];
            archive.Entries.CopyTo(entriesArray, 0);

            for (var i = 0; i < entriesArray.Length; i++)
            {
                var entry = entriesArray[i];
                if (entry.IsDirectory) continue;
                var name = Path.GetFileName(entry.Key);
                var fullPath = entry.Key;
                if (name == null || fullPath == null) continue;
                var result = ProcessArchiveEntry(fullPath, name, () => entry.OpenEntryStream());
                if (result != string.Empty)
                    data[fullPath] = result;
            }
        }

        _inputData.Set(data);
        return _inputData;
    }

    /// <summary>
    /// Обрабатывает один файл внутри архива, проверяя его расширение и извлекая содержимое в виде строки.
    /// </summary>
    /// <param name="fullPath">Полный путь к файлу внутри архива.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <param name="openStream">Функция, открывающая поток для чтения файла.</param>
    /// <returns>Содержимое файла в виде строки или пустая строка, если файл невалиден.</returns>
    private string ProcessArchiveEntry(string fullPath, string fileName, Func<Stream> openStream)
    {
        if (!IsValidFile(fileName) || fullPath.EndsWith('/')) return string.Empty;
        using var ms = new MemoryStream();
        using (var entryStream = openStream())
        {
            entryStream.CopyTo(ms);
        }

        ms.Seek(0, SeekOrigin.Begin);

        var reader = new StreamReader(ms, Encoding.UTF8);
        var result = reader.ReadToEnd();

        return FindFileType(result) ? result : string.Empty;
    }

    /// <summary>
    /// Проверяет, является ли файл допустимым для обработки на основе его расширения.
    /// </summary>
    /// <param name="filename">Имя файла, которое необходимо проверить.</param>
    /// <returns>True, если файл имеет допустимое расширение, иначе false.</returns>
    private bool IsValidFile(string filename)
    {
        if (filename == string.Empty) return false;

        var ext = new FileInfo(filename).Extension.ToLower().TrimStart('.');

        return !_unsupported.Contains(ext);
    }

    /// <summary>
    /// Проверяет, является ли содержимое файла слоем.
    /// </summary>
    /// <param name="file">Содержимое файла, которое необходимо проверить.</param>
    /// <returns>True, если содержимое файла удовлетворяет условию слоя, иначе false.</returns>
    private bool FindFileType(string file)
    {
        return file.Contains("%FS") || file.Contains("M48");
    }
}