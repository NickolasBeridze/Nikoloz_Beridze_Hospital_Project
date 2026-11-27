using System.Globalization;
using System.Text.Json;
using Hospital_Project.Enums;
using Hospital_Project.Exceptions;
using Hospital_Project.Services;
using Spectre.Console;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Helpers
{
    internal static class ConsoleUI
    {
        public const string DateFormat = "yyyy-MM-dd HH:mm";

        public static void ShowSuccess(string message)
        {
            AnsiConsole.MarkupLine($"[green]✔ {Markup.Escape(message)}[/]");
        }

        public static void ShowError(string message)
        {
            AnsiConsole.Write(
                new Panel($"[bold red]✖ {Markup.Escape(message)}[/]")
                    .BorderColor(Color.Red)
                    .RoundedBorder());
        }

        private static readonly List<string> DeferredWarnings = [];
        public static bool DeferWarnings { get; set; }

        public static void ShowWarning(string message)
        {
            if (DeferWarnings)
            {
                DeferredWarnings.Add(message);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]⚠ {Markup.Escape(message)}[/]");
        }

        public static void FlushWarnings()
        {
            DeferWarnings = false;

            foreach (string warning in DeferredWarnings)
                ShowWarning(warning);

            DeferredWarnings.Clear();
        }

        public static void ShowWelcome()
        {
            AnsiConsole.Write(
                new FigletText(T("HOSPITAL"))
                    .Centered()
                    .Color(Color.Cyan1));

            AnsiConsole.Write(
                new Panel(
                    Align.Center(
                        new Markup(
                            $"[bold white]{Markup.Escape(T("Hospital Management System"))}[/]\n" +
                            $"[grey]{Markup.Escape(T("Patient • Doctor • Administration"))}[/]\n" +
                            $"[white]{Markup.Escape(T("Made By Nikoloz Beridze"))}[/]"),
                        VerticalAlignment.Middle))
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Cyan1)
                    .Padding(2, 1));
        }

        public static async Task RunSafeAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception_Class ex)
            {
                ShowError(ex.Message);
            }
            catch (JsonException)
            {
                ShowError(T("A data file could not be read."));
            }
            catch (IOException)
            {
                ShowError(T("A file error occurred. Please try again."));
            }
            catch (UnauthorizedAccessException)
            {
                ShowError(T("The program has no permission to use a data file."));
            }
            catch (Exception ex)
            {
                ShowError($"{T("Unexpected error:")} {ex.Message}");
            }
        }

        public static string AskSecret(string prompt)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>(prompt)
                    .Secret()
                    .AllowEmpty());
        }

        public static string AskText(string prompt)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>(prompt)
                    .Validate(text => string.IsNullOrWhiteSpace(text)
                        ? ValidationResult.Error($"[red]{Markup.Escape(T("This field cannot be empty."))}[/]")
                        : ValidationResult.Success())).Trim();
        }

        public static Blood_Type_ENUMS AskBloodType(string title)
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<Blood_Type_ENUMS>()
                    .Title(title)
                    .PageSize(10)
                    .UseConverter(BloodTypeHelper.ToText)
                    .AddChoices(Enum.GetValues<Blood_Type_ENUMS>()));
        }

        public static int AskPositiveInt(string prompt)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<int>(prompt)
                    .ValidationErrorMessage($"[red]{Markup.Escape(T("Please enter a valid number."))}[/]")
                    .Validate(number => number > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error($"[red]{Markup.Escape(T("Please enter a valid number."))}[/]")));
        }

        public static decimal AskAmount(string prompt)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<decimal>(prompt)
                    .ValidationErrorMessage($"[red]{Markup.Escape(T("Please enter a valid number."))}[/]")
                    .Validate(amount => ToResult(Validator.CheckMoney(amount))));
        }

        public static decimal AskHours(string prompt)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<decimal>(prompt)
                    .ValidationErrorMessage($"[red]{Markup.Escape(T("Please enter a valid number."))}[/]")
                    .Validate(hours => ToResult(Validator.CheckHours(hours))));
        }

        public static DateTime AskDateTime(string prompt)
        {
            string text = AnsiConsole.Prompt(
                new TextPrompt<string>(prompt)
                    .Validate(input =>
                    {
                        if (!TryParseDate(input, out DateTime parsed))
                            return ValidationResult.Error(
                                $"[red]{Markup.Escape(T("Use this format:"))} {DateFormat}[/]");

                        return ToResult(AppointmentService.CheckDate(parsed));
                    }));

            TryParseDate(text, out DateTime result);
            return result;
        }

        public static bool TryParseDate(string? text, out DateTime result)
        {
            return DateTime.TryParseExact(
                text?.Trim(),
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result);
        }

        private static ValidationResult ToResult(string? error)
        {
            return error is null
                ? ValidationResult.Success()
                : ValidationResult.Error($"[red]{Markup.Escape(error)}[/]");
        }

        public static async Task ShowLoadingAsync(Dictionary<string, Func<Task>> steps)
        {
            await AnsiConsole.Progress()
                .AutoClear(true)
                .StartAsync(async context =>
                {
                    foreach (KeyValuePair<string, Func<Task>> step in steps)
                    {
                        ProgressTask task = context.AddTask(step.Key);

                        await FillAsync(task, 55);
                        await step.Value();
                        await FillAsync(task, 100);
                    }
                });
        }

        private static async Task FillAsync(ProgressTask task, double target)
        {
            while (task.Value < target)
            {
                task.Increment(2.5);
                await Task.Delay(20);
            }
        }
    }
}
