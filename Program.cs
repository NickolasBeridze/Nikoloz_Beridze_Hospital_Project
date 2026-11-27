using System.Text;
using Hospital_Project.Helpers;
using Hospital_Project.Menus;

namespace Hospital_Project;

internal class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        try
        {
            Translator.ChooseLanguage();
            ConsoleUI.ShowWelcome();

            HospitalApp app = new();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowError(ex.Message);
        }
    }
}
