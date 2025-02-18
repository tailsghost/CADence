using CADence.Infrastructure.LayerFabric.Common.Abstractions;
using CADence.Infrastructure.LayerFabric.Enums;
using CADence.Infrastructure.LayerFabric.Fabrics.Abstractions;
using CADence.Infrastructure.Parser.Parsers;
using CADence.Infrastructure.Parser.Parsers.Drills;
using CADence.Layer.Abstractions;
using CADence.Layer.Gerber_274x;
using CADence.Models.Format;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Utilities;
using SharpCompress.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    /// <summary>
    /// Список с готовыми слоями
    /// </summary>
    private List<LayerBase> _result = new();


    public async Task<List<LayerBase>> GetLayers(IInputData inputData)
    {
        return await Init(inputData.Get());
    }

    /// <summary>
    /// Инициализирует слои, проходя по данным и определяя каждый файл.
    /// </summary>
    /// <param name="data">Словарь с данными, где ключ - имя файла, а значение - его содержимое.</param>
    /// <returns>Список слоев, определенных из входных данных.</returns>
    private async Task<List<LayerBase>> Init(IDictionary<string, string> data)
    {
        var dataCopy = new Dictionary<string, string>(data);
        List<string> removeKeys = new();

        foreach (var kvp in data)
        {
            if (DetermineBoard(kvp.Key, kvp.Value) || DetermineDrill(kvp.Value))
            {
                removeKeys.Add(kvp.Key);
            }
        }

        for (var i = 0; i < removeKeys.Count; i++)
        {
            dataCopy.Remove(removeKeys[i]);
        }

        var substrate = new Substrate(new LayerFormat(), new DrillParser274X(_drills), new GerberParser274X(_outline));
        _result.Add(substrate);

        string fileTopCopper = GetFileString(Layer274xFileExtensionsSupported.gtl, dataCopy);
        string fileBottomCopper = GetFileString(Layer274xFileExtensionsSupported.gbl, dataCopy);
        string fileTopMask = GetFileString(Layer274xFileExtensionsSupported.gts, dataCopy);
        string fileBottomMask = GetFileString(Layer274xFileExtensionsSupported.gbs, dataCopy);
        string fileTopSilk = GetFileString(Layer274xFileExtensionsSupported.gto, dataCopy);
        string fileBottomSilk = GetFileString(Layer274xFileExtensionsSupported.gbo, dataCopy);

        Task<LayerBase> topCopperTask = CreateTopCopper(fileTopCopper, substrate);
        Task<LayerBase> bottomCopperTask = CreateBottomCopper(fileBottomCopper, substrate);
        Task<LayerBase> topMaskTask = CreateTopMask(fileTopMask, substrate);
        Task<LayerBase> bottomMaskTask = CreateBottomMask(fileBottomMask, substrate);

        Task<LayerBase> topSilkTask = CreateTopSilk(fileTopSilk, topMaskTask);
        Task<LayerBase> bottomSilkTask = CreateBottomSilk(fileBottomSilk, bottomCopperTask);

        Task<LayerBase> topFinishTask = CreateTopFinish(topCopperTask, topMaskTask);
        Task<LayerBase> bottomFinishTask = CreateBottomFinish(bottomCopperTask, bottomMaskTask);


        _tasks.AddRange(topMaskTask, bottomMaskTask, topCopperTask, bottomCopperTask, topSilkTask, bottomSilkTask, topFinishTask, bottomFinishTask);

        var result = await Task.WhenAll(_tasks);

        _result.AddRange(result);

        return _result;
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


    private string GetFileString(Layer274xFileExtensionsSupported fileType, Dictionary<string, string> data)
    {
        string extension = "." + fileType.ToString();

        var fileEntry = data.FirstOrDefault(kvp =>
            Path.GetExtension(kvp.Key).Equals(extension, StringComparison.OrdinalIgnoreCase));

        //if (fileEntry.Equals(default(KeyValuePair<string, string>)))
        //{
        //    //throw new ArgumentException($"Файл с расширением {extension} не найден в коллекции.");
        //}

        return fileEntry.Value;
    }


    private Task<LayerBase> CreateTopCopper(string file, Substrate substrate) =>
        Task.Run<LayerBase>(() => new TopCopper(new LayerFormat(), new GerberParser274X(file), substrate, true));

    private Task<LayerBase> CreateBottomCopper(string file, Substrate substrate) =>
        Task.Run<LayerBase>(() => new BottomCopper(new LayerFormat(), new GerberParser274X(file), substrate, true));



    private Task<LayerBase> CreateTopMask(string file, Substrate substrate) =>
        Task.Run<LayerBase>(() => new TopMask(new LayerFormat(), new GerberParser274X(file), substrate));



    private Task<LayerBase> CreateBottomMask(string file, Substrate substrate) =>
        Task.Run<LayerBase>(() => new BottomMask(new LayerFormat(), new GerberParser274X(file), substrate));


    private async Task<LayerBase> CreateTopSilk(string file, Task<LayerBase> topMaskTask)
    {
        LayerBase topMask = await topMaskTask;
        var topSilkTask = Task.Run<LayerBase>(() => new TopSilk(new LayerFormat(), new GerberParser274X(file), topMask.GetLayer()));
        return await topSilkTask;
    }

    private async Task<LayerBase> CreateTopFinish(Task<LayerBase> topCopperTask, Task<LayerBase> topMaskTask)
    {
        LayerBase topCopper = await topCopperTask;
        LayerBase topMask = await topMaskTask;
        var topFinishTask = Task.Run<LayerBase>(() => new TopFinish(new LayerFormat(), null, topMask.GetLayer(), topCopper.GetLayer()));
        return await topFinishTask;
    }

    private async Task<LayerBase> CreateBottomSilk(string file, Task<LayerBase> bottomCopperTask)
    {
        LayerBase bottomCopper = await bottomCopperTask;
        var bottomSilkTask = Task.Run<LayerBase>(() => new BottomSilk(new LayerFormat(), new GerberParser274X(file), bottomCopper.GetLayer()));
        return await bottomSilkTask;
    }

    private async Task<LayerBase> CreateBottomFinish(Task<LayerBase> bottomCopperTask, Task<LayerBase> bottomMaskTask)
    {
        LayerBase bottomCopper = await bottomCopperTask;
        LayerBase bottomMask = await bottomMaskTask;
        var bottomFinishTask = Task.Run<LayerBase>(() => new BottomFinish(new LayerFormat(), null, bottomMask.GetLayer(), bottomCopper.GetLayer()));
        return await bottomFinishTask;
    }
}