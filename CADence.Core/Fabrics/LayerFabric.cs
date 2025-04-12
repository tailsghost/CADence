using System.Reflection.Metadata.Ecma335;
using CADence.Abstractions.Layers;
using CADence.Abstractions.Readers;
using CADence.App.Abstractions.Layers;
using CADence.App.Abstractions.Layers.Gerber_274x;
using CADence.App.Abstractions.Parsers;
using ExtensionClipper2.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Fabrics
{
    /// <summary>
    /// Layer factory for Gerber 274x files. Creates various board layers based on the input data.
    /// </summary>
    internal class LayerFabricGerber274x : ILayerFabric
    {
        private readonly IServiceProvider _provider;

        public LayerFabricGerber274x(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// List of file formats supported for the outline.
        /// </summary>
        private readonly List<string> _boardSupported = new List<string> { "gko", "gm1", "gt3" };

        /// <summary>
        /// List of drill files used for forming the substrate layer.
        /// </summary>
        private List<string> _drills = new();

        /// <summary>
        /// Outline file content used for forming the substrate layer.
        /// </summary>
        private string _outline = string.Empty;

        /// <summary>
        /// List of tasks that process each layer.
        /// </summary>
        private readonly List<Task<ILayer>> _tasks = new();

        /// <summary>
        /// List of the final processed layers.
        /// </summary>
        private List<ILayer> _result = new();

        /// <summary>
        /// Gets layers based on the input data.
        /// </summary>
        /// <param name="inputData">Input data containing file contents.</param>
        /// <returns>A list of processed layers.</returns>
        public async Task<List<ILayer>> GetLayers(IInputData inputData)
        {
            return await Init(inputData.Get());
        }

        /// <summary>
        /// Initializes board layers by processing input files.
        /// </summary>
        /// <param name="data">Dictionary of file names and their content.</param>
        /// <returns>A list of board layers.</returns>
        private async Task<List<ILayer>> Init(IDictionary<string, string> data)
        {
            var dataCopy = new Dictionary<string, string>(data);
            var removeKeys = new List<string>();

            foreach (var key in data.Keys.ToList())
            {
                var value = data[key];
                if (DetermineBoard(key, value) || DetermineDrill(value))
                {
                    removeKeys.Add(key);
                }
            }

            foreach (var key in removeKeys)
            {
                dataCopy.Remove(key);
            }

            var substrate = CreateSubstrate(_drills, _outline);
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
            var bottomSilkTask = CreateBottomSilk(fileBottomSilk, bottomMaskTask);

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

            _tasks.AddRange(tasksArray);
            var result = await Task.WhenAll(_tasks);

            _result.AddRange(result);
            return _result;
        }

        /// <summary>
        /// Determines if the file is a board outline file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="file">Content of the file.</param>
        /// <returns>True if the file is identified as the board outline; otherwise, false.</returns>
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
        /// Determines if the file is a drill file.
        /// </summary>
        /// <param name="file">Content of the file.</param>
        /// <returns>True if the file contains drill information; otherwise, false.</returns>
        private bool DetermineDrill(string file)
        {
            if (!file.Contains("M48"))
                return false;

            _drills.Add(file);
            return true;
        }

        /// <summary>
        /// Retrieves file content based on a specific file extension.
        /// </summary>
        /// <param name="fileType">The file extension type.</param>
        /// <param name="data">Dictionary containing file data.</param>
        /// <returns>The file content as a string.</returns>
        private string GetFileString(Layer274xFileExtensionsSupported fileType, Dictionary<string, string> data)
        {
            var extension = "." + fileType.ToString();
            var fileEntry = data.FirstOrDefault(kvp =>
                Path.GetExtension(kvp.Key).Equals(extension, StringComparison.OrdinalIgnoreCase));

            return fileEntry.Value;
        }

        private Substrate CreateSubstrate(List<string> drills, string outline)
        {
            var parserDrill = _provider.GetRequiredService<IDrillParser>().Execute(_drills);
            var parser = _provider.GetRequiredService<IGerberParser>().Execute(_outline);

            return new Substrate(parserDrill, parser);
        }
            

        private Task<ILayer> CreateTopCopper(string file, Substrate substrate) =>
            Task.Run(() =>
            {
                var layer = _provider.GetRequiredService<Func<Substrate, string, TopCopper>>();
                return (ILayer)layer(substrate, file);
            });

        private Task<ILayer> CreateBottomCopper(string file, Substrate substrate) =>
            Task.Run(() =>
            {
                var layer = _provider.GetRequiredService<Func<Substrate, string, BottomCopper>>();
                return (ILayer)layer(substrate, file);
            });

        private Task<ILayer> CreateTopMask(string file, Substrate substrate) =>
            Task.Run(() =>
            {
                var layer = _provider.GetRequiredService<Func<Substrate, string, TopMask>>();
                return (ILayer)layer(substrate, file);
            });

        private Task<ILayer> CreateBottomMask(string file, Substrate substrate) =>
            Task.Run(() =>
            {
                var layer = _provider.GetRequiredService<Func<Substrate, string, BottomMask>>();
                return (ILayer)layer(substrate, file);
            });

        private async Task<ILayer> CreateTopSilk(string file, Task<ILayer> topMaskTask)
        {
            var topMask = await topMaskTask;
            return await Task.Run(() =>
            {
                var layer = _provider.GetRequiredService<Func<TopMask, string, TopSilk>>();
                return (ILayer)layer((TopMask)topMask, file);
            });
        }

        private async Task<ILayer> CreateTopFinish(Task<ILayer> topCopperTask, Task<ILayer> topMaskTask)
        {
            var topCopper = await topCopperTask;
            var topMask = await topMaskTask;
            return await Task.Run(() =>
            {
                var layer = _provider.GetRequiredService<Func<TopMask, TopCopper, TopFinish>>();
                return (ILayer)layer((TopMask)topMask, (TopCopper)topCopper);
            });
        }

        private async Task<ILayer> CreateBottomSilk(string file, Task<ILayer> bottomMaskTask)
        {
            var bottomMask = await bottomMaskTask;
            return await Task.Run(() =>
            {
                var layer = _provider.GetRequiredService<Func<BottomMask, string, BottomSilk>>();
                return (ILayer)layer((BottomMask)bottomMask, file);
            });
        }

        private async Task<ILayer> CreateBottomFinish(Task<ILayer> bottomCopperTask, Task<ILayer> bottomMaskTask)
        {
            var bottomCopper = await bottomCopperTask;
            var bottomMask = await bottomMaskTask;
            return await Task.Run(() =>
            {
                var layer = _provider.GetRequiredService<Func<BottomMask, BottomCopper, BottomFinish>>();
                return (ILayer)layer((BottomMask)bottomMask, (BottomCopper)bottomCopper);
            });
        }
    }
}