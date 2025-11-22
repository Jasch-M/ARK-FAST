using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ArkaZilla.Modules.Logging;

public class LoggingConfigurationService
{
    public class LoggingElementAutocomplete : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            List<IParameterInfo> parameterInfos = parameter.Command.Parameters.ToList();
            if (parameterInfos.Count == 0)
            {
                return Task.FromResult(AutocompletionResult.FromSuccess([]));
            }

            return parameterInfos[0] switch
            {

            };
        }
    }
}
