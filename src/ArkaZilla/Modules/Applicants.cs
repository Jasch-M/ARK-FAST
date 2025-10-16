using ArkaZilla.Modals.GameStudios;
using ArkaZilla.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace ArkaZilla.Modules;

public class Applicants(IOptions<ReferenceOptions> options) : ModuleBase
{
    [SlashCommand("accept", "Accept an application.")]
    public async Task AcceptApplication()
    {
        if (Context.Guild is null)
        {
            await RespondAsync("This command can only be used in the ARKAD Game Dev server", ephemeral: true);
            return;
        }

        if (Context.Guild.Id != ApplicationsHandler.DevGuildId)
        {
            await RespondAsync("This command can only be used in the ARKAD Game Dev server", ephemeral: true);
            return;
        }

        if (Context.User is not SocketGuildUser user)
        {
            await RespondAsync("This command can only be used in the ARKAD Game Dev server", ephemeral: true);
            return;
        }

        if (user.Roles.Any(role => role.Id == ApplicationsHandler.ApplicantRoleId))
        {
            await RespondAsync("<:ARKCute:1236821760263848046>\n" +
                "You are already an applicant. You can't accept yourself's application into the organizations", ephemeral: true);
            return;
        }

        if (user.Roles.All(role => role.Id != ApplicationsHandler.RecruiterRoleId))
        {
            await RespondAsync("You are not a recruiter. You can't accept applications", ephemeral: true);
            return;
        }

        if (Context.Channel is not SocketGuildChannel channel)
        {
            await RespondAsync("This command can only be used in an applicant's application discussion channel", ephemeral: true);
            return;
        }

        if (channel is not SocketTextChannel textChannel)
        {
            await RespondAsync("This command can only be used in an applicant's application discussion channel", ephemeral: true);
            return;
        }

        if (textChannel.CategoryId is not ApplicationsHandler.DevGuildApplicationCategoryId)
        {
            await RespondAsync("This command can only be used in an applicant's application discussion channel", ephemeral: true);
            return;
        }

        if (textChannel.Name.EndsWith(ApplicationsHandler.ApplicationChannelMarker))
        {
            await RespondAsync("This command can only be used in an applicant's application discussion channel", ephemeral: true);
            return;
        }

        string username = textChannel.Name.Replace(ApplicationsHandler.ApplicationChannelMarker, "");

        if (InteractionHandler.Bot is null)
        {
            // Should be impossible
            await RespondAsync(
                "A big problem occured while executing the command and I cannot continue executing because the client object was null", ephemeral: true);
            return; // TODO: Via logging also
        }

        SocketUser socketUser = InteractionHandler.Bot.GetUser(username);
        if (socketUser is null)
        {
            await RespondAsync("This command can only be used in an applicant's application discussion channel\n" +
                               "The user was null for some reason when fetched", ephemeral: true);
            return;
        }

        SocketGuild devGuild = InteractionHandler.Bot.GetGuild(ApplicationsHandler.DevGuildId);
        if (devGuild is null)
        {
            await RespondAsync(
                "A big problem occured while executing the command and I cannot continue executing because" +
                " the dev guild object was null", ephemeral: true);
            return;
        }

        SocketGuildUser guildUser = devGuild.GetUser(socketUser.Id);
        if (guildUser is null)
        {
            await RespondAsync("This applicant doesn't seem to be on the server. I can't continue");
            return;
        }

        await ApplicationsHandler.Instance.AcceptUser(guildUser);
    }

    [SlashCommand("cli", "Access the command line interface for ArkaZilla")]
    public async Task Cli(string command = "help")
    {
        if (command == "nakano")
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "npm";
            process.StartInfo.Arguments = "start";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = "/Users/jaschamerle/WebstormProjects/netsuki";
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardInput = false;

            await RespondAsync("Started Nakano Process");

            process.Exited += async (sender, args) =>
            {
                await Context.User.SendMessageAsync("Nakano Process Exited\nThank you for using me today!");
            };

            bool successful = process.Start();
            if (!successful)
            {
                await RespondAsync("Failed to start Nakano Process");
            }

            return;
        }

        const string pesterCeoCommand = "pester ceo ";
        if (command.StartsWith(pesterCeoCommand))
        {
            if (Context.User.Id != 557680932463706213)
            {
                await Context.User.SendMessageAsync("<:ARKCute:1236821760263848046>");
                await Context.User.SendMessageAsync("You are not the CEO of ARKA Zilla");
                await RespondAsync("Responded in your DMs", ephemeral: true);
                return;
            }

            await Context.User.SendMessageAsync("Pestered CEO");
            const ulong ceoId = 359004851788578816;
            var ceo = InteractionHandler.Bot?.GetUserAsync(ceoId) ?? null;

            if (ceo is null)
            {
                await Context.User.SendMessageAsync("CEO not found");
                await RespondAsync("Responded in your DMs", ephemeral: true);
                return;
            }

            while (ceo.Value.IsCompleted)
            {
                await Task.Delay(1000);
            }

            if (!ceo.Value.IsCompletedSuccessfully)
            {
                await Context.User.SendMessageAsync("CEO not found");
                await RespondAsync("Responded in your DMs", ephemeral: true);
                return;
            }

            await ceo.Value.Result.SendMessageAsync("Monsieur le ceo:\n" + command[pesterCeoCommand.Length..]);
            await RespondAsync("Executed the pester ceo command!", ephemeral: true);
            return;
        }

        await RespondAsync("Coming soon", ephemeral: true);
    }
}
