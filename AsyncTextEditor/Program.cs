using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTextEditor
{
    class Program
    {
        static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("1. Edit a text file");
                Console.WriteLine("2. Exit");
                Console.Write("Choose an option: ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await EditFileAsync();
                        break;
                    case "2":
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static async Task EditFileAsync()
        {
            Console.Write("Enter the path of the text file: ");
            string filePath = Console.ReadLine();

            try
            {
                await semaphore.WaitAsync();

                if (File.Exists(filePath)==false)
                {
                    Console.WriteLine("File not found.");
                    return;
                }

                using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string content = await reader.ReadToEndAsync();
                        Console.WriteLine($"Editing file: {filePath}");
                        Console.WriteLine("Enter your edits below (press Ctrl+S to save and exit):");

                        StringBuilder editedContent = new StringBuilder(content);
                        Console.WriteLine(content);
                        while (true)
                        {
                            string line = Console.ReadLine();
                            if (line == "^S") // Ctrl+S to save and exit
                            {
                                SaveAndExit(filePath, editedContent.ToString());
                                break;
                            }
                            else
                            {
                                editedContent.AppendLine(line);
                            }
                        }
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        static void SaveAndExit(string filePath, string content)
        {
            try
            {
                using (FileStream fileStream = File.Open(filePath, FileMode.Truncate, FileAccess.Write, FileShare.Read))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.Write(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
            Console.WriteLine("Changes saved. Exiting editor.");
        }
    }
}
