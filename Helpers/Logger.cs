using System.Text;
using Hospital_Project.Services;
using Spectre.Console;

namespace Hospital_Project.Helpers
{
    internal static class Logger
    {
        public static Action<string>? OnEvent;

        public static void Log(string message) => OnEvent?.Invoke(message);

        public static void WriteToFile(string message)
        {
            try
            {
                Directory.CreateDirectory(FileService.DataFolder);

                using StreamWriter writer = new(FileService.LogPath, true, new UTF8Encoding(false));
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}");
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static void WriteToConsole(string message)
        {
            AnsiConsole.MarkupLine($"[grey]• {Markup.Escape(message)}[/]");
        }
    }
}
