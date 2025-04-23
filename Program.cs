using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// 1. Обобщённая функция расширения для поиска максимального элемента
public static class CollectionExtensions
{
    public static T GetMax<T>(this IEnumerable<T> collection, Func<T, float> convertToNumber) where T : class
    {
        if (collection == null || !collection.Any())
        {
            return null; // Или выбросить исключение, если коллекция не может быть пустой
        }

        T maxElement = collection.First();
        float maxValue = convertToNumber(maxElement);

        foreach (var element in collection.Skip(1))
        {
            float currentValue = convertToNumber(element);
            if (currentValue > maxValue)
            {
                maxValue = currentValue;
                maxElement = element;
            }
        }

        return maxElement;
    }
}

// 2. Аргументы события FileFound
public class FileArgs : EventArgs
{
    public string FileName { get; }
    public bool Cancel { get; set; } // Для отмены дальнейшего поиска

    public FileArgs(string fileName)
    {
        FileName = fileName;
        Cancel = false;
    }
}

// 3. Класс, обходящий каталог файлов и выдающий событие
public class FileSearcher
{
    public event EventHandler<FileArgs> FileFound;

    public void Search(string directory)
    {
        try
        {
            foreach (string filePath in Directory.GetFiles(directory))
            {
                var args = new FileArgs(filePath);
                OnFileFound(args);

                if (args.Cancel)
                {
                    Console.WriteLine("Поиск отменен обработчиком события.");
                    return;
                }
            }

            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                Search(subDirectory); // Рекурсивный обход подкаталогов
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обходе каталога {directory}: {ex.Message}");
        }
    }

    protected virtual void OnFileFound(FileArgs e)
    {
        FileFound?.Invoke(this, e);
    }
}

// 4. Пример использования
class Program
{
    static void Main(string[] args)
    {
        string directoryToSearch = "."; // Текущая директория

        //  Поиск максимального файла по размеру
        var files = Directory.GetFiles(directoryToSearch, "*.*", SearchOption.AllDirectories).ToList();

        Func<string, float> convertToSize = (filePath) => new FileInfo(filePath).Length;

        string maxFile = files.GetMax(convertToSize);

        if (maxFile != null)
        {
            Console.WriteLine($"Максимальный файл по размеру: {maxFile}");
        }
        else
        {
            Console.WriteLine("Нет файлов для анализа.");
        }

        // Используем FileSearcher
        var searcher = new FileSearcher();

        // Подписываемся на событие
        searcher.FileFound += (sender, args) =>
        {
            Console.WriteLine($"Найден файл: {args.FileName}");

            // Пример отмены поиска после нахождения определенного файла
            if (args.FileName.Contains("important"))
            {
                Console.WriteLine("Обнаружен важный файл. Отмена дальнейшего поиска.");
                args.Cancel = true;
            }
        };

        // Запускаем поиск
        Console.WriteLine($"\nНачинаем поиск файлов в директории: {directoryToSearch}");
        searcher.Search(directoryToSearch);

        Console.WriteLine("Поиск завершен.");
        Console.ReadKey();
    }
}