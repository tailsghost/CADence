namespace CADence.CUI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string? answer = string.Empty;
            string? path = string.Empty;
            while (true)
            {
                while (true)
                {
                    Console.Write("Введите путь к файлу: ");
                    path = Console.ReadLine();
                    Console.Write("Подтвердите ввод [y/n]: ");
                    answer = Console.ReadLine();


                    if (answer == "y")
                    {
                        break;
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

                Console.Write("Хотите продолжить? [y/n]: ");
                answer = Console.ReadLine();

                if (answer == "y")
                {
                    break;
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

        public static void Execute(string? path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }


        }
    }
}