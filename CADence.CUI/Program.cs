using System.Diagnostics;
using CADence.App.Abstractions.Layers;
using CADence.Core.Dependency;
using CADence.Abstractions.Readers;
using CADence.Abstractions.Svg_Json;
using CADence.Abstractions.Global;
using CADence.Abstractions.Helpers;
using CADence.Abstractions.Layers;

namespace CADence.CUI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            ServiceCollectionExtensions.Initial();

            while (true)
            {
                var path = GetFilePathFromUser();
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }


                AskUserForAccuracy();

                Console.WriteLine("Parsing file...");
                try
                {
                    await ExecuteAsync(path);
                    Console.WriteLine("Parsing completed successfully.");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                    throw;
                }

                if (!AskUserToContinue())
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Asks the user whether to execute the accuracy calculation.
        /// </summary>
        private static void AskUserForAccuracy()
        {
            while (true)
            {
                Console.Write("Do you want to calculate accuracy? [y/n]: ");
                string? answer = Console.ReadLine();
                if (answer?.ToLower() == "y")
                {
                    ExecuteAccuracy.SetExecute(true);
                    break;
                }
                else if (answer?.ToLower() == "n")
                {
                    ExecuteAccuracy.SetExecute(false);
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Asks the user for the file path and confirmation.
        /// </summary>
        /// <returns>The file path if confirmed; otherwise, null.</returns>
        private static string? GetFilePathFromUser()
        {
            while (true)
            {
                Console.Write("Enter the file path: ");
                string? path = Console.ReadLine();
                Console.Write("Confirm input [y/n]: ");
                string? answer = Console.ReadLine();

                if (answer?.ToLower() == "y")
                {
                    return path;
                }
                else if (answer?.ToLower() == "n")
                {
                    Console.Clear();
                    continue;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Press any key to continue...");
                    Console.ResetColor();
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        /// <summary>
        /// Asks the user if they want to continue processing files.
        /// </summary>
        /// <returns>True if the user wants to continue; otherwise, false.</returns>
        private static bool AskUserToContinue()
        {
            while (true)
            {
                Console.Write("Do you want to continue? [y/n]: ");
                string? answer = Console.ReadLine();

                if (answer?.ToLower() == "y")
                {
                    Console.Clear();
                    return true;
                }
                else if (answer?.ToLower() == "n")
                {
                    return false;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Press any key to continue...");
                    Console.ResetColor();
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        /// <summary>
        /// Executes file parsing for the given file path.
        /// </summary>
        /// <param name="path">The path to the file (e.g., C:\WorkSpace\Example.zip).</param>
        public static async Task ExecuteAsync(string? path)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            List<ILayer> layers;
            using (var stream = new MemoryStream(File.ReadAllBytes(path)))
            {
                var reader = ServiceCollectionExtensions.GetService<IReader>();
                var data = reader.ParseArchive(stream, Path.GetFileName(path));
                var fabric = ServiceCollectionExtensions.GetService<ILayerFabric>();
                layers = await fabric.GetLayers(data);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            var directory = Path.GetDirectoryName(path) ?? throw new ArgumentNullException(nameof(path));
            var outputPathBack = Path.Combine(directory, "outputBack.svg");
            var outputPathFront = Path.Combine(directory, "outputFront.svg");
            Console.WriteLine($"SVG file will be saved to: {outputPathBack}");
            Console.WriteLine($"SVG file will be saved to: {outputPathFront}");

            var svgWriter = ServiceCollectionExtensions.GetService<IWriter>();

            var accuracyTask = Task.Run(async () =>
            {
                if (ExecuteAccuracy.GetExecute())
                {
                    var coppers = layers.OfType<ICopper>().ToList();
                    var box = CalculateAccuracyHelper.Execute(await coppers[0].GetAccuracy(),
                                                              await coppers[1].GetAccuracy());
                    Console.WriteLine($"Minimum distance from hole to outline: {box.DistanceFromHoleToOutline}");
                    Console.WriteLine($"Minimum distance between tracks: {box.DistanceBetweenTracks}");
                }
            });

            var svgTask = Task.Run(() =>
            {
                svgWriter.Execute(layers, 2, true, outputPathBack);
                svgWriter.Execute(layers, 2, false, outputPathFront);
            });

            Task.WaitAll(accuracyTask, svgTask);

            stopwatch.Stop();
            var memoryUsage = GC.GetTotalMemory(false);
            Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds} ms, Memory used: {memoryUsage} bytes");

            Console.ResetColor();
        }
    }
}