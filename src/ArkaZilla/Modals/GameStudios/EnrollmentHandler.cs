using Discord;
using Discord.WebSocket;

namespace ArkaZilla.Modals.GameStudios;

public class EnrollmentHandler : IButtonHandler
{
    public string ButtonId => "enroll-button";

    public const string ModalId = "arkad-game-studios-enrollment-form";

    private const string ModalTitle = "Enroll as a learner";

    internal const string ExperienceInputLabel = "What is your experience with programming?";

    internal const string ExperienceInputId = "arkad-game-studios-enrollment-experience";

    internal const string LearnInputLabel = "What do you want to learn?";

    internal const string LearnInputId = "arkad-game-studios-enrollment-what-do-you-want-to-learn";

    internal const string SpecialNeedsInputLabel = "Is there anything else you would like to say?";

    internal const string SpecialNeedsInputId = "arkad-game-studios-enrollment-special-needs";

    public async Task HandleButton(SocketMessageComponent component)
    {
        ModalBuilder modalBuilder = new ModalBuilder()
            .WithTitle(ModalTitle)
            .WithCustomId(ModalId)
            .AddTextInput(
                ExperienceInputLabel,
                ExperienceInputId,
                TextInputStyle.Paragraph,
                required: true)
            .AddTextInput(
                LearnInputLabel,
                LearnInputId,
                TextInputStyle.Paragraph,
                required: true)
            .AddTextInput(
                SpecialNeedsInputLabel,
                SpecialNeedsInputId,
                TextInputStyle.Paragraph,
                required: false);

        await component.RespondWithModalAsync(modalBuilder.Build());
    }
}
