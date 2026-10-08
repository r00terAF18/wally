using Spectre.Console;

namespace wally.Core;

/// <summary>
/// Status output for the non-interactive commands. It goes to stderr so stdout only carries
/// results (file paths), which keeps wally scriptable: wally set "$(wally get cats)".
/// </summary>
public static class Log
{
    public static IAnsiConsole Err { get; } =
        AnsiConsole.Create(new AnsiConsoleSettings { Out = new AnsiConsoleOutput(Console.Error) });

    public static void Info(string message) => Err.MarkupLineInterpolated($"[green][[+]][/] {message}");
    public static void Warn(string message) => Err.MarkupLineInterpolated($"[yellow][[!]][/] {message}");
    public static void Error(string message) => Err.MarkupLineInterpolated($"[red][[x]][/] {message}");
}
