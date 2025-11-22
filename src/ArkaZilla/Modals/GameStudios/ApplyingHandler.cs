using Discord;
using Discord.WebSocket;

namespace ArkaZilla.Modals.GameStudios;

public class ApplyingHandler : IButtonHandler
{
    public string ButtonId => "apply-button";


    internal const string ModalId = "arkad-game-studios-applying-form";

    internal const string WhyArkadQuestion = "Why are you applying to ARKAD Game Studios?";

    internal const string WhyArkadId = "arkad-game-studios-applying-why-arkad";

    internal const string OfferQuestion = "What do you bring to the table for us?";

    internal const string OfferId = "arkad-game-studios-applying-offer";

    internal const string ExperienceQuestion = "What is your experience?";

    internal const string ExperienceId = "arkad-game-studios-applying-experience";

    internal const string SpecialNeedsQuestion = "Is there anything else you would like to say?";

    internal const string SpecialNeedsId = "arkad-game-studios-applying-special-needs";

    public async Task HandleButton(SocketMessageComponent component)
    {
        ModalBuilder modalBuilder = new ModalBuilder()
            .WithTitle("Apply to be apart of ARKAD Game Studios")
            .WithCustomId(ModalId)
            .AddTextInput(
                WhyArkadQuestion,
                WhyArkadId,
                TextInputStyle.Paragraph,
                required: true)
            .AddTextInput(
                OfferQuestion,
                OfferId,
                TextInputStyle.Paragraph,
                required: true)
            .AddTextInput(
                ExperienceQuestion,
                ExperienceId,
                TextInputStyle.Paragraph,
                required: false)
            .AddTextInput(
                SpecialNeedsQuestion,
                SpecialNeedsId,
                TextInputStyle.Paragraph,
                required: false);

        // Show the modal
        await component.RespondWithModalAsync(modalBuilder.Build());
    }
}
