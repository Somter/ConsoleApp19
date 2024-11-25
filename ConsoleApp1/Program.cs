using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace CSharpApplication.SearchInFiles
{
    class Search
    {
        static void Main()
        {
            List<string> foundItems = new List<string>();
            string startDirectory;
            Regex searchMask;
           

            if (!SelectSearchArea(out startDirectory))
            {
                return;
            }

            if (!SpecifyMask(out searchMask))
            {
                return;
            }

            ExecuteSearch(startDirectory, searchMask, foundItems);

            if (foundItems.Count > 0)
            {
                DisplayResults(foundItems);
                HandleActions(foundItems);
            }
           
        }

        static bool SelectSearchArea(out string startDirectory)
        {
            Console.WriteLine("Выберите область поиска:");
            Console.WriteLine("1. Искать в указанной директории");
            Console.WriteLine("2. Искать на всех дисках");
            string choice = Console.ReadLine();

            startDirectory = "";

            if (choice == "1")
            {
                Console.Write("Введите путь для директории: ");
                startDirectory = Console.ReadLine();

                if (startDirectory.Length > 0 && startDirectory[^1] != Path.DirectorySeparatorChar)
                {
                    startDirectory += Path.DirectorySeparatorChar;
                }

                if (!Directory.Exists(startDirectory))
                {
                    Console.WriteLine("Неверный путь");
                    return false;
                }
            }

            return true;
        }

        static bool SpecifyMask(out Regex searchMask)
        {
            Console.Write("Введите маску файлов: ");
            string mask = Console.ReadLine();

            mask = Regex.Escape(mask).Replace(@"\*", ".*").Replace(@"\?", ".");
            searchMask = new Regex("^" + mask + "$", RegexOptions.IgnoreCase);
            return true;
        }

        static void ExecuteSearch(string directory, Regex mask, List<string> results)
        {
            if (!string.IsNullOrEmpty(directory))
            {
                SearchInDirectory(directory, mask, results);
            }
            else
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        SearchInDirectory(drive.RootDirectory.FullName, mask, results);
                    }
                }
            }
        }

        static void SearchInDirectory(string path, Regex mask, List<string> results)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(path))
                {
                    if (mask.IsMatch(Path.GetFileName(file)))
                    {
                        results.Add(file);
                    }
                }

                foreach (var subdirectory in Directory.EnumerateDirectories(path))
                {
                    if (mask.IsMatch(Path.GetFileName(subdirectory)))
                    {
                        results.Add(subdirectory);
                    }

                    SearchInDirectory(subdirectory, mask, results);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void DisplayResults(List<string> results)
        {
            Console.WriteLine("\nРезултат:");
            for (int i = 0; i < results.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {results[i]}");
            }
        }

        static void HandleActions(List<string> results)
        {
            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("1. Удалить все элементы");
            Console.WriteLine("2. Удалить выбранный элемент");
            Console.WriteLine("3. Удалить диапазон элементов");
            Console.WriteLine("4. Завершить работу");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DeleteAll(results);
                    break;
                case "2":
                    DeleteSingle(results);
                    break;
                case "3":
                    DeleteRange(results);
                    break;
                case "4":
                    Console.WriteLine("Программа завершена.");
                    break;
                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }
        }

        static void DeleteAll(List<string> results)
        {
            Console.Write("Вы уверены, что хЩотите удалить все элементы? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                foreach (var item in results)
                    DeleteItem(item);
                Console.WriteLine("Элементы удалены.");
            }
        }

        static void DeleteSingle(List<string> results)
        {
            Console.Write("Введите номер элемента для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= results.Count)
            {
                DeleteItem(results[index - 1]);
                Console.WriteLine("Элемент удален");
            }
            else
            {
                Console.WriteLine("Неверный номер элемента.");
            }
        }

        static void DeleteRange(List<string> results)
        {
            Console.Write("Введите начальный номер диапазона: ");
            if (int.TryParse(Console.ReadLine(), out int startIndex))
            {
                Console.Write("Введите конечный номер диапазона: ");
                if (int.TryParse(Console.ReadLine(), out int endIndex) &&
                    startIndex >= 1 && endIndex <= results.Count && startIndex <= endIndex)
                {
                    Console.Write($"Удалить элементы с {startIndex} по {endIndex}? (y/n): ");
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        for (int i = startIndex - 1; i < endIndex; i++)
                            DeleteItem(results[i]);
                        Console.WriteLine("Элементы удалены.");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный диапазон.");
                }
            }
            else
            {
                Console.WriteLine("Неверный номер.");
            }
        }

        static void DeleteItem(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
