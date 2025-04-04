using System.Diagnostics;
using CADence.App.Abstractions.Layers;
using CADence.Core.Dependency;
using CADence.Abstractions.Readers;
using CADence.Abstractions.Layers;
using CADence.Abstractions.Svg_Json;
using ExtensionClipper2;
using CADence.Abstractions.Global;
using CADence.Abstractions.Helpers;

namespace CADence.CUI
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            ServiceCollectionExtensions.Initial();
            Epsilon.SetEpsilonValue(1e-10);
            ExecuteAccuracy.SetExecute(true);

            while (true)
            {
                string? path = GetFilePathFromUser();
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                Console.WriteLine("Парсинг файла...");
                try
                {
                    await ExecuteAsync(path);
                    Console.WriteLine("Парсинг успешно завершён.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }

                if (!AskUserToContinue())
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Запрашивает у пользователя путь к файлу и подтверждение.
        /// </summary>
        /// <returns>Путь к файлу или null, если ввод не подтвержден.</returns>
        private static string? GetFilePathFromUser()
        {
            while (true)
            {
                Console.Write("Введите путь к файлу: ");
                string? path = Console.ReadLine();
                Console.Write("Подтвердите ввод [y/n]: ");
                string? answer = Console.ReadLine();

                if (answer == "y")
                {
                    return path;
                }

                if (answer == "n")
                {
                    Console.Clear();
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ввод. Нажмите любую кнопку чтобы продолжить...");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
            }
        }

        /// <summary>
        /// Спрашивает у пользователя, хочет ли он продолжить.
        /// </summary>
        /// <returns>true, если пользователь хочет продолжить; иначе false.</returns>
        private static bool AskUserToContinue()
        {
            while (true)
            {
                Console.WriteLine("Хотите продолжить? [y/n]: ");
                string? answer = Console.ReadLine();

                if (answer == "y")
                {
                    Console.Clear();
                    return true;
                }

                if (answer == "n")
                {
                    return false;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некорректный ввод. Нажмите любую кнопку чтобы продолжить...");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.ReadKey();
                Console.Clear();
            }
        }

        /// <summary>
        /// Выполняет парсинг файла по указанному пути.
        /// </summary>
        /// <param name="path">Путь к файлу. Пример: C:\WorkSpace\M3CITY2REV0Gerber.zip</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если путь пустой или null.</exception>
        public static async Task ExecuteAsync(string? path)
        {
            Stopwatch wather = new();
            wather.Start();

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            List<ILayer> layers;

            var file = File.ReadAllBytes(path);
            using (var stream = new MemoryStream(file))
            {
                var reader = ServiceCollectionExtensions.GetService<IReader>();
                var data = reader.ParseArchive(stream, Path.GetFileName(path));
                var fabric = ServiceCollectionExtensions.GetService<ILayerFabric>();
                layers = await fabric.GetLayers(data);
            }

                Console.ForegroundColor = ConsoleColor.Green;
                var pathToSVGWritingFront =
                    Path.Combine(Path.GetDirectoryName(path) ?? throw new ArgumentNullException(nameof(path)),
                        "outputFront.svg");
                Console.WriteLine($"Путь по которому будет сохранён файл: {pathToSVGWritingFront}");
                var pathToSVGWritingBack =
                    Path.Combine(Path.GetDirectoryName(path) ?? throw new ArgumentNullException(nameof(path)),
                        "outputBack.svg");
                Console.WriteLine($"Путь по которому будет сохранён файл: {pathToSVGWritingBack}");

                var svgWriter = ServiceCollectionExtensions.GetService<IWriter>();

                var task1 = Task.Run(async () =>
                {
                    if (ExecuteAccuracy.GetExecute())
                    {
                        var coppers = layers.OfType<ICopper>().ToList();
                        var box = CalculateAccuracyHelper.Execute(await coppers[0].GetAccuracy(),
                            await coppers[1].GetAccuracy());

                        Console.WriteLine(
                            $"Минимальное расстояние от дырки до ободка: {box.DistanceFromHoleToOutline}");
                        Console.WriteLine($"Минимальное расстояние между полигонами: {box.DistanceBetweenTracks}");
                    }
                });

                var task2 = Task.Run(() =>
                {
                    svgWriter.Execute(layers, 2, true, pathToSVGWritingBack);
                    svgWriter.Execute(layers, 2, false, pathToSVGWritingFront);
                });

                Task.WaitAll(task1, task2);

                wather.Stop();
                var memoryUsage = GC.GetTotalMemory(false);
                Console.WriteLine($"Общее время выполнения {wather.ElapsedMilliseconds} мс, Использовано памяти: {memoryUsage} байт");

                Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
