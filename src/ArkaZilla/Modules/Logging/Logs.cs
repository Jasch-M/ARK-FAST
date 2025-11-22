using ArkaZilla.Modules.Logging;
using Discord.Interactions;
using Microsoft.Extensions.Options;

namespace ArkaZilla.Modules.Logging;

public class Logs(IOptions<ReferenceOptions> options) : ModuleBase
{
    [SlashCommand("logging-display-configuration", "Displays the current logging configuration.")]
    public async Task DisplayLoggingConfiguration(
        [Summary("configurable-element", "Logging configuration for")] LoggingConfigurationCategory selection,
        [Autocomplete(typeof(LoggingConfigurationService.LoggingElementAutocomplete))] string element)
    {
        await RespondAsync("Not yet implemented.", ephemeral: true);
    }

    [SlashCommand("configure-logging", "Configures the logging system.")]
    public async Task ConfigureLogging(string selection)
    { }

    public enum LoggingConfigurationCategory
    {
        [ChoiceDisplay("servers")]
        Servers,

        [ChoiceDisplay("categories")]
        Categories,

        [ChoiceDisplay("channels")]
        Channels,

        [ChoiceDisplay("channel")]
        Channel
    }
}
