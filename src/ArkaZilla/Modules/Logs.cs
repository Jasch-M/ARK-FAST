using ArkaZilla.Modules.Logging;
using Discord.Interactions;
using Microsoft.Extensions.Options;

namespace ArkaZilla.Modules;

public class Logs(IOptions<ReferenceOptions> options) : ModuleBase
{
    [SlashCommand("logging-display-configuration", "Displays the current logging configuration.")]
    public async Task DisplayLoggingConfiguration(
        [Choice("servers", "servers"),
         Choice("categories", "categories"),
         Choice("channels", "channels"),
         Choice("channel", "channel"),
         Summary("Logging configuration for")] string selection,
        [Autocomplete(typeof(LoggingConfigurationService.LoggingElementAutocomplete))] string element)
    {

    }

    [SlashCommand("configure-logging", "Configures the logging system.")]
    public async Task ConfigureLogging(string)
    { }
}
