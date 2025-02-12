using CADence.Infrastructure.LayerFabric.Fabrics.Gerber274x;
using CADence.Infrastructure.LayerFabric.Readers;

namespace CADence.CUI
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
                    Execute(path);
                    Console.WriteLine("Парсинг успешно завершён.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при выполнении парсинга: {ex.Message}");
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

                Console.WriteLine("Некорректный ввод.\r\nНажмите любую кнопку чтобы продолжить...");
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
                    return true;
                }

                if (answer == "n")
                {
                    return false;
                }

                Console.WriteLine("Некорректный ввод.\r\nНажмите любую кнопку чтобы продолжить...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        /// <summary>
        /// Выполняет парсинг файла по указанному пути.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если путь пустой или null.</exception>
        public static void Execute(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            byte[] file = File.ReadAllBytes(path);
            var stream = new MemoryStream(file);
            BoardFileReader reader = new();
            var data = reader.ParseArchive(stream, Path.GetFileName(path));
            LayerFabricGerber274x fabric = new();
            fabric.GetLayers(data);
        }
    }
}
