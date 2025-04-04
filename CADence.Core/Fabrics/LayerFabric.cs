using CADence.Abstractions.Layers;
using CADence.Abstractions.Readers;
using CADence.App.Abstractions.Layers;
using CADence.App.Abstractions.Layers.Gerber_274x;
using CADence.App.Abstractions.Parsers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CADence.Core.Fabrics
{
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
        private readonly List<string> _boardSupported = new List<string> { "gko", "gm1", "gt3" };

        /// <summary>
        /// Список дырок, нужны для формирования слоя Substrate.
        /// </summary>
        private List<string> _drills = new();

        /// <summary>
        /// Outline, нужен для формирования слоя Substrate.
        /// </summary>
        private string _outline = string.Empty;

        /// <summary>
        /// Список задач, в котором содержится информация о каждом слое.
        /// </summary>
        private readonly List<Task<ILayer>> _tasks = new List<Task<ILayer>>();

        /// <summary>
        /// Список с готовыми слоями.
        /// </summary>
        private List<ILayer> _result = new List<ILayer>();

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
            var removeKeys = new List<string>();

            var keys = data.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                string value = data[key];
                if (DetermineBoard(key, value) || DetermineDrill(value))
                {
                    removeKeys.Add(key);
                }
            }

            for (var i = 0; i < removeKeys.Count; i++)
            {
                dataCopy.Remove(removeKeys[i]);
            }

            var substrate = new Substrate(
                _provider.GetRequiredService<IDrillParser>().Execute(_drills),
                _provider.GetRequiredService<IGerberParser>().Execute(_outline)
            );
            _result.Add(substrate);

            var fileTopCopper = GetFileString(Layer274xFileExtensionsSupported.gtl, dataCopy);
            var fileBottomCopper = GetFileString(Layer274xFileExtensionsSupported.gbl, dataCopy);
            var fileTopMask = GetFileString(Layer274xFileExtensionsSupported.gts, dataCopy);
            var fileBottomMask = GetFileString(Layer274xFileExtensionsSupported.gbs, dataCopy);
            var fileTopSilk = GetFileString(Layer274xFileExtensionsSupported.gto, dataCopy);
            var fileBottomSilk = GetFileString(Layer274xFileExtensionsSupported.gbo, dataCopy);

            var topCopperTask = CreateTopCopper(fileTopCopper, substrate);
            var bottomCopperTask = CreateBottomCopper(fileBottomCopper, substrate);
            var topMaskTask = CreateTopMask(fileTopMask, substrate);
            var bottomMaskTask = CreateBottomMask(fileBottomMask, substrate);

            var topSilkTask = CreateTopSilk(fileTopSilk, topMaskTask);
            var bottomSilkTask = CreateBottomSilk(fileBottomSilk, bottomCopperTask);

            var topFinishTask = CreateTopFinish(topCopperTask, topMaskTask);
            var bottomFinishTask = CreateBottomFinish(bottomCopperTask, bottomMaskTask);


            var tasksArray = new Task<ILayer>[]
            {
                topMaskTask,
                bottomMaskTask,
                topCopperTask,
                bottomCopperTask,
                topSilkTask,
                bottomSilkTask,
                topFinishTask,
                bottomFinishTask
            };

            for (var i = 0; i < tasksArray.Length; i++)
            {
                _tasks.Add(tasksArray[i]);
            }

            var result = await Task.WhenAll(_tasks);

            for (var i = 0; i < result.Length; i++)
            {
                _result.Add(result[i]);
            }

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
            if (!string.IsNullOrEmpty(_outline))
                return false;

            var ext = Path.GetExtension(fileName).TrimStart('.').ToLower();
            if (!_boardSupported.Contains(ext))
                return false;

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
            if (!file.Contains("M48"))
                return false;

            _drills.Add(file);
            return true;
        }

        /// <summary>
        /// Получает содержимое файла по заданному типу расширения.
        /// </summary>
        /// <param name="fileType">Тип расширения файла.</param>
        /// <param name="data">Словарь с данными.</param>
        /// <returns>Строка с содержимым файла.</returns>
        private string GetFileString(Layer274xFileExtensionsSupported fileType, Dictionary<string, string> data)
        {
            var extension = "." + fileType.ToString();
            var fileEntry = data.FirstOrDefault(kvp =>
                Path.GetExtension(kvp.Key).Equals(extension, System.StringComparison.OrdinalIgnoreCase));

            return fileEntry.Value;
        }

        private Task<ILayer> CreateTopCopper(string file, Substrate substrate) =>
            Task.Run(() => _provider.GetRequiredService<TopCopper>().Init(new[] { substrate.GetLayer() }, file));

        private Task<ILayer> CreateBottomCopper(string file, Substrate substrate) =>
            Task.Run(() => _provider.GetRequiredService<BottomCopper>().Init(new[] { substrate.GetLayer() }, file));

        private Task<ILayer> CreateTopMask(string file, Substrate substrate) =>
            Task.Run(() => _provider.GetRequiredService<TopMask>().Init(new[] { substrate.GetLayer() }, file));

        private Task<ILayer> CreateBottomMask(string file, Substrate substrate) =>
            Task.Run(() => _provider.GetRequiredService<BottomMask>().Init(new[] { substrate.GetLayer() }, file));

        private async Task<ILayer> CreateTopSilk(string file, Task<ILayer> topMaskTask)
        {
            var topMask = await topMaskTask;
            return await Task.Run(() => _provider.GetRequiredService<TopSilk>().Init(new[] { topMask.GetLayer() }, file)); ;
        }

        private async Task<ILayer> CreateTopFinish(Task<ILayer> topCopperTask, Task<ILayer> topMaskTask)
        {
            var topCopper = await topCopperTask;
            var topMask = await topMaskTask;
            var topFinishTask = Task.Run(() => _provider.GetRequiredService<TopFinish>().Init(new[] { topMask.GetLayer(), topCopper.GetLayer() }));
            return await topFinishTask;
        }

        private async Task<ILayer> CreateBottomSilk(string file, Task<ILayer> bottomMaskTask)
        {
            var bottomMask = await bottomMaskTask;
            var bottomSilkTask = Task.Run(() => _provider.GetRequiredService<BottomSilk>().Init(new[] { bottomMask.GetLayer() }, file));
            return await bottomSilkTask;
        }

        private async Task<ILayer> CreateBottomFinish(Task<ILayer> bottomCopperTask, Task<ILayer> bottomMaskTask)
        {
            var bottomCopper = await bottomCopperTask;
            var bottomMask = await bottomMaskTask;
            var bottomFinishTask = Task.Run(() => _provider.GetRequiredService<BottomFinish>().Init(new[] { bottomMask.GetLayer(), bottomCopper.GetLayer() }));
            return await bottomFinishTask;
        }
    }
}