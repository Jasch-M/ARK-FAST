using Discord;
using Discord.Interactions;

namespace ArkaZilla.Modules.Logging;

public class LoggingConfigurationService
{
    public class LoggingElementAutocomplete : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            throw new NotImplementedException();
        }
    }
}
