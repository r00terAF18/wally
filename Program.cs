using System.CommandLine;
using wally.Cli;
using wally.Core;

// "wally" alone opens the interactive menu; see "wally --help" for the commands.
try
{
    return await WallyCli.Build().Parse(args)
        .InvokeAsync(new InvocationConfiguration { EnableDefaultExceptionHandler = false });
}
catch (ConfigException ex)
{
    // A broken config file should be a one-line error, not a stack trace.
    Log.Error(ex.Message);
    return 1;
}
