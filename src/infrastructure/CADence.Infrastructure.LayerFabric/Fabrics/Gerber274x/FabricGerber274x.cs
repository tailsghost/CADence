using CADence.Infrastructure.LayerFabric.Common.Abstractions;
using CADence.Infrastructure.LayerFabric.Fabrics.Abstractions;
using CADence.Infrastructure.Parser.Parsers;
using CADence.Infrastructure.Parser.Parsers.Drills;
using CADence.Layer.Abstractions;
using CADence.Layer.Gerber_274x;
using CADence.Models.Format;

namespace CADence.Infrastructure.LayerFabric.Fabrics.Gerber274x;

public class FabricGerber274X : IFabric
{
    /// <summary>
    /// Список дырок, нужны для формирования слоя Substrate.
    /// </summary>
    private List<string> _drills = [];

    /// <summary>
    /// Outline, нужен для формирования слоя Substrate.
    /// </summary>
    private string _outline = string.Empty;

    /// <summary>
    /// Список задач, в котором содержится информация о каждом слое.
    /// </summary>
    private readonly List<Task<LayerBase>> _tasks = new();

    /// <summary>
    /// Получает слои из данных, переданных через IInputData.
    /// </summary>
    /// <param name="inputData">Объект, предоставляющий входные данные.</param>
    /// <returns>Список слоев, соответствующих входным данным.</returns>
    public List<LayerBase> GetLayers(IInputData inputData)
    {
        return Init(inputData.Get()).Result;
    }

    /// <summary>
    /// Инициализирует слои, проходя по данным и определяя каждый файл.
    /// </summary>
    /// <param name="data">Словарь с данными, где ключ - имя файла, а значение - его содержимое.</param>
    /// <returns>Список слоев, определенных из входных данных.</returns>
    private async Task<List<LayerBase>> Init(IDictionary<string, string> data)
    {
        for (var i = 0; i < data.Count; i++)
        {
            var key = data.Keys.ElementAt(i);
            var value = data[key];

            if (DetermineBoard(key, value))
                continue;

            if (DetermineDrill(value))
                continue;

            _tasks.Add(DetermineLayer(key, value));
        }

        var resultTask = await Task.WhenAll(_tasks);
        var result = resultTask.ToList();
        result.Insert(0,
            new Substrate(new ApertureFormat(), new GerberParserDrill274X(_drills), new GerberParser274X(_outline)));
        return result;
    }

    /// <summary>
    /// Определяет, является ли файл файлом с контуром платы.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    /// <param name="file">Содержимое файла.</param>
    /// <returns>True, если файл является файлом с контуром, иначе false.</returns>
    private bool DetermineBoard(string fileName, string file)
    {
        if (_outline != string.Empty) return false;
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLower();

        if (ext != "gko" && ext != "gm1" && ext != "gt3") return false;
        _outline = file;
        
        return true;
    }

    /// <summary>
    /// Определяет, является ли файл файлом с отверстиями.
    /// </summary>
    /// <param name="file">Содержимое файла.</param>
    /// <returns>True, если файл содержит отверстия, иначе false.</returns>
    private bool DetermineDrill(string file)
    {
        if (!file.Contains("M48")) return false;
        _drills.Add(file);
        return true;
    }

    /// <summary>
    /// Определяет слой по расширению файла и создает соответствующий объект слоя.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    /// <param name="file">Содержимое файла.</param>
    /// <returns>Задача, которая при завершении возвращает объект слоя.</returns>
    private Task<LayerBase> DetermineLayer(string fileName, string file)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLower();

        Task result = ext switch
        {
            "gbl" => Task.Run(() => new BottomCopper(new ApertureFormat(), new GerberParser274X(file))),
            "gtl" => Task.Run(() => new TopCopper(new ApertureFormat(), new GerberParser274X(file))),
            "gbo" => Task.Run(() => new BottomSilk(new ApertureFormat(), new GerberParser274X(file))),
            "gto" => Task.Run(() => new TopSilk(new ApertureFormat(), new GerberParser274X(file))),
            "gbs" => Task.Run(() => new BottomMask(new ApertureFormat(), new GerberParser274X(file))),
            "gts" => Task.Run(() => new TopMask(new ApertureFormat(), new GerberParser274X(file))),
            _ => throw new ArgumentOutOfRangeException()
        };

        return (Task<LayerBase>)result;
    }
}