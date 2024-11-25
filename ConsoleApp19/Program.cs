using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CSharpApplication.SearchInFiles
{
    class Search
    {
        static void Main()
        {
            Console.Write("Введите путь к каталогу: ");
            string directoryPath = Console.ReadLine();
            Console.Write("Введите маску файла: ");
            string fileMask = Console.ReadLine();
            Console.Write("Введите дату начала(год, месяц, день): ");
            string startDateInput = Console.ReadLine();
            Console.Write("Введите дату окончания (год, месяц, день): ");
            string endDateInput = Console.ReadLine();

            DirectoryInfo directory = new DirectoryInfo(directoryPath);
           
            fileMask = Regex.Escape(fileMask);
            fileMask = fileMask.Replace(@"\*", ".*");
            fileMask = fileMask.Replace(@"\?", ".");
            fileMask = "^" + fileMask + "$";

            Regex maskRegex = new Regex(fileMask, RegexOptions.IgnoreCase);

            DateTime startDate, endDate;
            if (!DateTime.TryParseExact(startDateInput, "(год, месяц, день)", null, System.Globalization.DateTimeStyles.None, out startDate))
            {
                Console.WriteLine("Ошибка с датой");
                return;
            }

            if (!DateTime.TryParseExact(endDateInput, "(год, месяц, день)", null, System.Globalization.DateTimeStyles.None, out endDate))
            {
                Console.WriteLine("Ошибка датой");
                return;
            }

            using (StreamWriter reportWriter = new StreamWriter("report.txt"))
            {
                ulong totalFiles = SearchFiles(directory, maskRegex, startDate, endDate, reportWriter);
                Console.WriteLine("Найдено файлов: {0}.", totalFiles);
            }
            Console.WriteLine("Данные поиска записаны в report.txt");
           
        }

        static ulong SearchFiles(DirectoryInfo directory, Regex maskRegex, DateTime startDate, DateTime endDate, StreamWriter reportWriter)
        {
            ulong fileCount = 0;
            FileInfo[] files = null;
            try
            {
                files = directory.GetFiles();
            }
            catch
            {
                return fileCount;
            }

            foreach (FileInfo file in files)
            {
                if (maskRegex.IsMatch(file.Name))
                {
                    DateTime lastModified = file.LastWriteTime;
                    if (lastModified.Date >= startDate.Date && lastModified.Date <= endDate.Date)
                    {
                        ++fileCount;
                        reportWriter.WriteLine("Файл: {0}, Последнее изменение: {1}", file.FullName, lastModified);
                    }
                }
            }

            DirectoryInfo[] subdirectories = null;
            try
            {
                subdirectories = directory.GetDirectories();
            }
            catch
            {
                return fileCount;
            }

            foreach (DirectoryInfo subdirectory in subdirectories)
            {
                fileCount += SearchFiles(subdirectory, maskRegex, startDate, endDate, reportWriter);
            }

            return fileCount;
        }
    }
}
