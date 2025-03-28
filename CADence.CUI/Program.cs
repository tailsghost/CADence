using System.Diagnostics;
using CADence.App.Abstractions.Layers;
using NLog;
using CADence.Core.SVG_JSON;
using CADence.Core.Readers;
using CADence.Core.Dependency;
using CADence.Abstractions.Readers;
using CADence.Abstractions.Layers;
using CADence.Abstractions.Svg_Json;

namespace CADence.CUI
{
    internal class Program
    {
        // Получаем логгер для текущего класса
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            ServiceCollectionExtensions.Initial();
            // Настройка NLog: вывод в консоль и запись в файл
            var config = new NLog.Config.LoggingConfiguration();

            // Консольный таргет
            var logConsole = new NLog.Targets.ConsoleTarget("logConsole")
            {
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=toString}"
            };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);

            // Файловый таргет
            var logFile = new NLog.Targets.FileTarget("logFile")
            {
                FileName = "log.txt",
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=toString}"
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logFile);

            LogManager.Configuration = config;

            while (true)
            {
                string? path = GetFilePathFromUser();
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                _logger.Info("Парсинг файла...");
                await ExecuteAsync(path);
                _logger.Info("Парсинг успешно завершён.");

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

                _logger.Warn("Некорректный ввод. Нажмите любую кнопку чтобы продолжить...");
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
                Console.Write("Хотите продолжить? [y/n]: ");
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

                _logger.Warn("Некорректный ввод. Нажмите любую кнопку чтобы продолжить...");
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


            var pathToSVGWritingFront = Path.Combine(Path.GetDirectoryName(path) ?? throw new ArgumentNullException(nameof(path)), "outputFront.svg");
            _logger.Info($"Путь по которому будет сохранён файл: {pathToSVGWritingFront}");
            var pathToSVGWritingBack = Path.Combine(Path.GetDirectoryName(path) ?? throw new ArgumentNullException(nameof(path)), "outputBack.svg");
            _logger.Info($"Путь по которому будет сохранён файл: {pathToSVGWritingBack}");

            var svgWriter = ServiceCollectionExtensions.GetService<IWriter>();
            svgWriter.Execute(layers, 2,true, pathToSVGWritingBack);
            svgWriter.Execute(layers, 2,false, pathToSVGWritingFront);
            wather.Stop();
            _logger.Info($"Общее время выполнения {wather.ElapsedMilliseconds / 1000.0} секунд");

        }
    }
}
