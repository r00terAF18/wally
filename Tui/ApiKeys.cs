using Spectre.Console;
using wally.Core;

namespace wally.Tui;

public static class ApiKeys
{
    /// <summary>
    /// Interactive-only fallback when no Pexels key is configured: ask for it and offer to
    /// save it to the config file. The CLI never prompts; it errors with instructions instead.
    /// </summary>
    public static void EnsurePexelsKey()
    {
        AppConfig config = ConfigStore.Current;
        if (!string.IsNullOrWhiteSpace(config.Pexels.ApiKey))
            return;

        string apiKey = AnsiConsole.Prompt(
            new TextPrompt<string>("Pexels API key ([grey]free at https://www.pexels.com/api/[/]):").Secret()).Trim();

        if (AnsiConsole.Confirm($"Save it to {Markup.Escape(ConfigStore.ConfigPath)}?"))
        {
            AppConfig file = ConfigStore.LoadFile();
            file.Pexels.ApiKey = apiKey;
            ConfigStore.Save(file);
        }

        config.Pexels.ApiKey = apiKey;
    }
}
