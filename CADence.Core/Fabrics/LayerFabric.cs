using CADence.Abstractions.Layers;
using CADence.Abstractions.Readers;
using CADence.App.Abstractions.Layers;
using CADence.App.Abstractions.Layers.Gerber_274x;
using CADence.App.Abstractions.Parsers;
using CADence.Core.Formats;
using CADence.Core.Parsers;
using Clipper2Lib;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Fabrics;

public class LayerFabricGerber274x : ILayerFabric
{
    private readonly IServiceProvider _provider;
    public LayerFabricGerber274x(IServiceProvider provider)
    {
        _provider = provider;
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
    private readonly List<Task<ILayer>> _tasks = new();

    /// <summary>
    /// Список с готовыми слоями
    /// </summary>
    private List<ILayer> _result = new();


    public async Task<List<ILayer>> GetLayers(IInputData inputData)
    {
        return await Init(inputData.Get());
    }

    /// <summary>
    /// Инициализирует слои, проходя по данным и определяя каждый файл.
    /// </summary>
    /// <param name="data">Словарь с данными, где ключ - имя файла, а значение - его содержимое.</param>
    /// <returns>Список слоев, определенных из входных данных.</returns>
    private async Task<List<ILayer>> Init(IDictionary<string, string> data)
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

        var substrate = new Substrate(_provider.GetRequiredService<IDrillParser>().Execute(_drills),
                                                        _provider.GetRequiredService<IGerberParser>().Execute(_outline));
        _result.Add(substrate);

        string fileTopCopper = GetFileString(Layer274xFileExtensionsSupported.gtl, dataCopy);
        string fileBottomCopper = GetFileString(Layer274xFileExtensionsSupported.gbl, dataCopy);
        string fileTopMask = GetFileString(Layer274xFileExtensionsSupported.gts, dataCopy);
        string fileBottomMask = GetFileString(Layer274xFileExtensionsSupported.gbs, dataCopy);
        string fileTopSilk = GetFileString(Layer274xFileExtensionsSupported.gto, dataCopy);
        string fileBottomSilk = GetFileString(Layer274xFileExtensionsSupported.gbo, dataCopy);

        Task<ILayer> topCopperTask = CreateTopCopper(fileTopCopper, substrate);
        Task<ILayer> bottomCopperTask = CreateBottomCopper(fileBottomCopper, substrate);
        Task<ILayer> topMaskTask = CreateTopMask(fileTopMask, substrate);
        Task<ILayer> bottomMaskTask = CreateBottomMask(fileBottomMask, substrate);

        Task<ILayer> topSilkTask = CreateTopSilk(fileTopSilk, topMaskTask);
        Task<ILayer> bottomSilkTask = CreateBottomSilk(fileBottomSilk, bottomCopperTask);

        Task<ILayer> topFinishTask = CreateTopFinish(topCopperTask, topMaskTask);
        Task<ILayer> bottomFinishTask = CreateBottomFinish(bottomCopperTask, bottomMaskTask);


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


    private Task<ILayer> CreateTopCopper(string file, Substrate substrate) =>
        Task.Run<ILayer>(() => _provider.GetRequiredService<TopCopper>().Init([substrate.GetLayer()], file));

    private Task<ILayer> CreateBottomCopper(string file, Substrate substrate) =>
        Task.Run<ILayer>(() => _provider.GetRequiredService<BottomCopper>().Init([substrate.GetLayer()], file));



    private Task<ILayer> CreateTopMask(string file, Substrate substrate) =>
        Task.Run<ILayer>(() => _provider.GetRequiredService<TopMask>().Init([substrate.GetLayer()], file));



    private Task<ILayer> CreateBottomMask(string file, Substrate substrate) =>
        Task.Run<ILayer>(() => _provider.GetRequiredService<BottomMask>().Init([substrate.GetLayer()], file));


    private async Task<ILayer> CreateTopSilk(string file, Task<ILayer> topMaskTask)
    {
        ILayer topMask = await topMaskTask;
        var topSilkTask = Task.Run<ILayer>(() => _provider.GetRequiredService<TopSilk>().Init([topMask.GetLayer()], file));
        return await topSilkTask;
    }

    private async Task<ILayer> CreateTopFinish(Task<ILayer> topCopperTask, Task<ILayer> topMaskTask)
    {
        ILayer topCopper = await topCopperTask;
        ILayer topMask = await topMaskTask;
        var topFinishTask = Task.Run<ILayer>(() => _provider.GetRequiredService<TopFinish>().Init([topMask.GetLayer(), topCopper.GetLayer()]));
        return await topFinishTask;
    }

    private async Task<ILayer> CreateBottomSilk(string file, Task<ILayer> bottomMaskTask)
    {
        ILayer bottomMask = await bottomMaskTask;
        var bottomSilkTask = Task.Run<ILayer>(() => _provider.GetRequiredService<BottomSilk>().Init([bottomMask.GetLayer()], file));
        return await bottomSilkTask;
    }

    private async Task<ILayer> CreateBottomFinish(Task<ILayer> bottomCopperTask, Task<ILayer> bottomMaskTask)
    {
        ILayer bottomCopper = await bottomCopperTask;
        ILayer bottomMask = await bottomMaskTask;
        var bottomFinishTask = Task.Run<ILayer>(() => _provider.GetRequiredService<TopFinish>().Init([bottomMask.GetLayer(), bottomCopper.GetLayer()]));
        return await bottomFinishTask;
    }
}
