using ArkaZilla.Services;
using Discord;
using Discord.WebSocket;

namespace ArkaZilla.Modals.GameStudios;

public class EnrollmentDealer : IModalHandler
{
    public string ModalId => "arkad-game-studios-enrollment-form";

    internal const ulong EnrollmentChannelId = 1404640896875757669;

    internal const ulong LearnersChannelId = 1394650301268623391;

    internal const ulong LearnerRoleId = 1395043983926759495;

    public async Task HandleModal(SocketModal modal)
    {
        Embed embed = GenerateEnrollmentEmbed(modal);

        if (InteractionHandler.Bot is
            null) // TODO: Upgrade Logging to be a singleton accessible to the whole code and add a simple mechanism to inform the developer if needed
        {
            await modal.User.SendMessageAsync("Hey sorry, a critical issue occured with the bot." +
                                              "I have logged this already for the developers.");
            return;
        }

        SocketGuild? guild = InteractionHandler.Bot.GetGuild(ApplicationsHandler.DevGuildId);
        if (guild is null)
        {
            await modal.User.SendMessageAsync("Hey sorry, a critical issue occured with the bot." +
                                              "I have logged this already for the developers.");
            return;
        }

        await guild.DownloadUsersAsync();
        SocketGuildUser? socketGuildUser = guild.GetUser(modal.User.Id);
        if (socketGuildUser is null)
        {
            await modal.User.SendMessageAsync("Hey sorry, a critical issue occured with the bot." +
                                              "I have logged this already for the developers.\n" +
                                              "I couldn't find you on ARKAD Game Studio's discord server");
            return;
        }

        if (socketGuildUser.Roles.Any(role => role.Id == LearnerRoleId))
        {
            await modal.User.SendMessageAsync("YOOOOOOO\n" +
                                              "Thanks for wanting to double down on your learning quest." +
                                              "You already have the role so if you want to tell us something more specific, " +
                                              "feel free to talk to an instructor or open a ticket with staff directly.");
            return;
        }

        SocketTextChannel? channel = guild.GetTextChannel(EnrollmentChannelId);
        if (channel is null)
        {
            return;
        }

        await modal.RespondAsync("Thanks for enrolling! You now have access to the learners side", ephemeral: true);
        await channel.SendMessageAsync(embed: embed);
        await socketGuildUser.AddRoleAsync(guild.GetRole(LearnerRoleId));

        SocketTextChannel? learnersChannel = guild.GetTextChannel(LearnersChannelId);
        if (learnersChannel is null)
        {
            return;
        }

        await learnersChannel.SendMessageAsync($"YO everyone look! {modal.User.Mention} just enrolled!\n" +
                                               $"Say hi to your fellow classmate!");
    }

    private Embed GenerateEnrollmentEmbed(SocketModal modal)
    {
        IReadOnlyCollection<SocketMessageComponentData> socketMessageComponents = modal.Data.Components;

        if (socketMessageComponents.Count != 3)
        {
            return GenerateErrorApplicationEmbed();
        }


        SocketMessageComponentData experience;
        SocketMessageComponentData learn;
        SocketMessageComponentData specialNeeds;

        try
        {
            experience =
                socketMessageComponents.First(message => message.CustomId == EnrollmentHandler.ExperienceInputId);
            learn = socketMessageComponents.First(message => message.CustomId == EnrollmentHandler.LearnInputId);
            specialNeeds =
                socketMessageComponents.First(message => message.CustomId == EnrollmentHandler.SpecialNeedsInputId);
        }
        catch (InvalidOperationException _)
        {
            return GenerateErrorApplicationEmbed();
        }

        EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle($"{modal.User.Mention}'s Enrollment Form")
            .AddField(new EmbedFieldBuilder()
                .WithName(EnrollmentHandler.ExperienceInputLabel)
                .WithValue(experience.Value))
            .AddField(new EmbedFieldBuilder()
                .WithName(EnrollmentHandler.LearnInputLabel)
                .WithValue(learn.Value));

        if (!string.IsNullOrWhiteSpace(specialNeeds.Value))
        {
            embedBuilder.AddField(new EmbedFieldBuilder()
                .WithName(ApplyingHandler.SpecialNeedsQuestion)
                .WithValue(specialNeeds.Value));
        }

        return embedBuilder.Build();
    }

    private Embed GenerateErrorApplicationEmbed()
    {
        return new EmbedBuilder().WithTitle("An error occured.")
            .WithDescription("I have informed the developers")
            .WithColor(Color.Red).Build();
    }
}
