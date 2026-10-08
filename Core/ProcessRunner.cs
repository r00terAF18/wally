using System.ComponentModel;
using System.Diagnostics;

namespace wally.Core;

public sealed record ProcessResult(int ExitCode, string StdOut, string StdErr)
{
    public bool Success => ExitCode == 0;

    /// <summary>The most useful error text: stderr, else stdout, else the exit code.</summary>
    public string Error =>
        !string.IsNullOrWhiteSpace(StdErr) ? StdErr.Trim()
        : !string.IsNullOrWhiteSpace(StdOut) ? StdOut.Trim()
        : $"exit code {ExitCode}";
}

/// <summary>
/// Runs external programs without a shell. Arguments always go through
/// ProcessStartInfo.ArgumentList, so file names with spaces, quotes or $(...) are passed
/// verbatim and can never be interpreted as extra commands.
/// </summary>
public static class ProcessRunner
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public static async Task<ProcessResult> RunAsync(string fileName, CancellationToken ct, params string[] arguments)
    {
        var info = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (string argument in arguments)
            info.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = info };
        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            return new ProcessResult(127, "", $"could not start {fileName}: {ex.Message}");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(DefaultTimeout);

        // Read both pipes concurrently so a chatty process can't dead-lock on a full buffer.
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        Task<string> stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            return new ProcessResult(process.ExitCode, await stdout, await stderr);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
            ct.ThrowIfCancellationRequested();
            return new ProcessResult(124, "", $"{fileName} timed out after {DefaultTimeout.TotalSeconds:0}s");
        }
    }

    public static Task<ProcessResult> RunAsync(string fileName, params string[] arguments) =>
        RunAsync(fileName, CancellationToken.None, arguments);

    /// <summary>
    /// Starts a long-running helper (e.g. swaybg) fully detached from wally: new session,
    /// stdio pointed at /dev/null, so it keeps running after wally exits. The program and its
    /// arguments are passed to sh as positional parameters ("$@"), never spliced into the script.
    /// </summary>
    public static async Task<ProcessResult> StartDetachedAsync(string fileName, CancellationToken ct, params string[] arguments)
    {
        string[] shellArgs = ["-c", "setsid -f \"$@\" </dev/null >/dev/null 2>&1", "wally-detach", fileName, .. arguments];
        return await RunAsync("sh", ct, shellArgs);
    }

    /// <summary>Returns the full path of <paramref name="program"/> if it is on PATH.</summary>
    public static string? FindOnPath(string program)
    {
        if (Path.IsPathRooted(program))
            return File.Exists(program) ? program : null;

        string[] extensions = OperatingSystem.IsWindows() ? [".exe", ".cmd", ".bat", ""] : [""];
        string path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (string dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        foreach (string ext in extensions)
        {
            string candidate = Path.Combine(dir, program + ext);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    public static bool IsOnPath(string program) => FindOnPath(program) is not null;

    public static bool IsRunning(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        foreach (Process p in processes) p.Dispose();
        return processes.Length > 0;
    }
}
