using CADence.Infrastructure.LayerFabric.Common.Abstractions;
using CADence.Infrastructure.LayerFabric.Enums;
using CADence.Infrastructure.LayerFabric.Fabrics.Abstractions;
using CADence.Infrastructure.Parser.Parsers;
using CADence.Infrastructure.Parser.Parsers.Drills;
using CADence.Layer.Abstractions;
using CADence.Layer.Gerber_274x;
using CADence.Models.Format;
using SharpCompress.Common;

namespace CADence.Infrastructure.LayerFabric.Fabrics.Gerber274x;

public class LayerFabricGerber274x : ILayerFabric
{
    private readonly NLog.ILogger _logger;
    public LayerFabricGerber274x()
    {
        _logger = NLog.LogManager.GetCurrentClassLogger();
    }
    /// <summary>
    /// Список форматов для <see cref="_outline"/>.
    /// </summary>
    private readonly List<string> _boardSupported = ["gko", "gm1", "gt3"];
    
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


    public async Task<List<LayerBase>> GetLayers(IInputData inputData)
    {
        try
        {
            return await Init(inputData.Get());
        }
        catch(Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    /// <summary>
    /// Инициализирует слои, проходя по данным и определяя каждый файл.
    /// </summary>
    /// <param name="data">Словарь с данными, где ключ - имя файла, а значение - его содержимое.</param>
    /// <returns>Список слоев, определенных из входных данных.</returns>
    private async Task<List<LayerBase>> Init(IDictionary<string, string> data)
    {
        try
        {
            var dataCopy = data;

            List<string> ids = new();

            for (var i = 0; i < dataCopy.Count; i++)
            {
                var key = data.Keys.ElementAt(i);
                var value = data[key];

                if (DetermineBoard(key, value))
                {
                    ids.Add(key);
                    continue;
                }

                if (DetermineDrill(value))
                {
                    ids.Add(key);
                    continue;
                }
            }

            for (var i = 0; i < ids.Count; i++)
            {
                var key = ids[i];
                dataCopy.Remove(key);
            }

            ids = null;


            var Substrate = new Substrate(new LayerFormat(), new DrillParser274X(_drills), new GerberParser274X(_outline));

            var resultTask = await Task.WhenAll(_tasks);
            var result = resultTask.ToList();

            for (var i = 0; i < dataCopy.Count; i++)
            {
                var key = data.Keys.ElementAt(i);
                var ext = Path.GetExtension(key).TrimStart('.').ToLower();

                if (!Enum.TryParse(ext, true, out Layer274xFileExtensionsSupported extension))
                {
                    throw new ArgumentOutOfRangeException();
                }


                var value = data[key];

                //_tasks.Add(DetermineLayer(key, value, Substrate));

                //if (extension == Layer274xFileExtensionsSupported.gbl)
                //{
                //    result.Add(new BottomCopper(new LayerFormat(), new GerberParser274X(value), Substrate));
                //}

                if (extension == Layer274xFileExtensionsSupported.gtl)
                {
                    result.Add(new TopCopper(new LayerFormat(), new GerberParser274X(value), Substrate));
                }
            }

            result.Insert(0, Substrate);
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
            throw new Exception(ex.Message, ex);
        }
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

        if (!_boardSupported.Contains(ext)) return false;
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

    // <summary>
    // Определяет слой по расширению файла и создает соответствующий объект слоя.
    // </summary>
    // <param name = "fileName" > Имя файла.</param>
    // <param name = "file" > Содержимое файла.</param>
    // <returns>Задача, которая при завершении возвращает объект слоя.</returns>
    private Task<LayerBase> DetermineLayer(string fileName, string file, Substrate substrate)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLower();

        if (!Enum.TryParse(ext, true, out Layer274xFileExtensionsSupported extension))
        {
            throw new ArgumentOutOfRangeException();
        }

        var result = extension switch
        {
            Layer274xFileExtensionsSupported.gbl => Task.Run<LayerBase>(() => new BottomCopper(new LayerFormat(), new GerberParser274X(file), substrate)),
            Layer274xFileExtensionsSupported.gtl => Task.Run<LayerBase>(() => new TopCopper(new LayerFormat(), new GerberParser274X(file), substrate)),
            //Layer274xFileExtensionsSupported.gbo => Task.Run<LayerBase>(() => new BottomSilk(new LayerFormat(), new GerberParser274X(file))),
            //Layer274xFileExtensionsSupported.gto => Task.Run<LayerBase>(() => new TopSilk(new LayerFormat(), new GerberParser274X(file))),
            //Layer274xFileExtensionsSupported.gbs => Task.Run<LayerBase>(() => new BottomMask(new LayerFormat(), new GerberParser274X(file))),
            //Layer274xFileExtensionsSupported.gts => Task.Run<LayerBase>(() => new TopMask(new LayerFormat(), new GerberParser274X(file))),
            //_ => throw new ArgumentOutOfRangeException()
        };

        return result;
    }
}