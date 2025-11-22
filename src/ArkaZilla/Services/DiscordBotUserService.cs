using System.Text;
using ArkaZilla.Extensions.Logging;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using ArkaZilla.Extensions;

namespace ArkaZilla.Services;

// IHostedService is the standard way to run background tasks in a .NET host.
public class DiscordBotUserService
    : IHostedService, IDiscordBotUserService
{
    // The bot instance will be injected here!
    private Timer? _timer;

    public DiscordLogger? Logger => DiscordLogger.Logger;

    public DiscordSocketClient Client { get; }

    // internal RoleAssociationService RoleAssociationService { get; private set; }

    private const ulong GuildId = 520752175031517195;

    private Dictionary<ulong, string> permissionNames = new Dictionary<ulong, string>()
    {
        { (ulong)GuildPermission.CreateInstantInvite, "Create Invites" },
        { (ulong)GuildPermission.KickMembers, "Kick Members" },
        { (ulong)GuildPermission.BanMembers, "Ban Members" },
        { (ulong)GuildPermission.Administrator, "Administrator" },
        { (ulong)GuildPermission.ManageChannels, "Manage Channels" },
        { (ulong)GuildPermission.ManageGuild, "Manage Server" },
        { (ulong)GuildPermission.AddReactions, "Add Reactions" },
        { (ulong)GuildPermission.ViewAuditLog, "View Audit Log" },
        { (ulong)GuildPermission.PrioritySpeaker, "Priority Speaker" },
        { (ulong)GuildPermission.Stream, "Stream" },
        { (ulong)GuildPermission.ViewChannel, "View Channel" },
        { (ulong)GuildPermission.SendMessages, "Send Messages" },
        { (ulong)GuildPermission.SendTTSMessages, "Send TTS Messages" },
        { (ulong)GuildPermission.ManageMessages, "Manage Messages" },
        { (ulong)GuildPermission.EmbedLinks, "Embed Links" },
        { (ulong)GuildPermission.AttachFiles, "Attach Files" },
        { (ulong)GuildPermission.ReadMessageHistory, "Read Message History" },
        { (ulong)GuildPermission.MentionEveryone, "Mention Everyone" },
        { (ulong)GuildPermission.UseExternalEmojis, "Use External Emojis" },
        { (ulong)GuildPermission.ViewGuildInsights, "View Server Insights" },
        { (ulong)GuildPermission.Connect, "Connect" },
        { (ulong)GuildPermission.Speak, "Speak" },
        { (ulong)GuildPermission.MuteMembers, "Mute Members" },
        { (ulong)GuildPermission.DeafenMembers, "Deafen Members" },
        { (ulong)GuildPermission.MoveMembers, "Move Members" },
        { (ulong)GuildPermission.UseVAD, "Use Voice Activity" },
        { (ulong)GuildPermission.ChangeNickname, "Change Nickname" },
        { (ulong)GuildPermission.ManageNicknames, "Manage Nicknames" },
        { (ulong)GuildPermission.ManageRoles, "Manage Roles" },
        { (ulong)GuildPermission.ManageWebhooks, "Manage Webhooks" },
        { (ulong)GuildPermission.ManageEmojisAndStickers, "Manage Emojis and Stickers" },
        { (ulong)GuildPermission.UseApplicationCommands, "Use Application Commands" },
        { (ulong)GuildPermission.RequestToSpeak, "Request to Speak" },
        { (ulong)GuildPermission.ManageEvents, "Manage Events" },
        { (ulong)GuildPermission.ManageThreads, "Manage Threads" },
        { (ulong)GuildPermission.CreatePublicThreads, "Create Public Threads" },
        { (ulong)GuildPermission.CreatePrivateThreads, "Create Private Threads" },
        { (ulong)GuildPermission.UseExternalStickers, "Use External Stickers" },
        { (ulong)GuildPermission.SendMessagesInThreads, "Send Messages in Threads" },
        { (ulong)GuildPermission.StartEmbeddedActivities, "Start Embedded Activities" },
        { (ulong)GuildPermission.ModerateMembers, "Moderate Members" },
        { (ulong)GuildPermission.ViewMonetizationAnalytics, "View Monetization Analytics" },
        { (ulong)GuildPermission.UseSoundboard, "Use Soundboard" },
        { (ulong)GuildPermission.CreateGuildExpressions, "Create Guild Expressions" },
        { (ulong)GuildPermission.SendVoiceMessages, "Send Voice Messages" },
        { (ulong)GuildPermission.UseClydeAI, "Use Clyde AI" },
        { (ulong)GuildPermission.SetVoiceChannelStatus, "Set Voice Channel Status" },
    };

    public DiscordBotUserService(ILogger<DiscordBotUserService> logger, DiscordSocketClient client)
    {
        DiscordLogger.Logger = new DiscordLogger(logger);
        Client = client;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        DiscordLogger.LogInformation("My Custom Service is starting.");

        // Wait until the client is ready before starting the timer.
        Client.Ready += OnClientReady;

        return Task.CompletedTask;
    }

    private Task OnClientReady()
    {
        // RoleAssociationService = new RoleAssociationService(this);
        SocketGuild? arkadGuild = Client.GetGuild(GuildId);
        if (arkadGuild is null)
        {
            DiscordLogger.LogCritical(GuildId, "Could not find the server for team ARKAD");
            return Task.CompletedTask;
        }


        SocketGuild? socketGuild = Client.GetGuild(GuildId);
        if (socketGuild is null)
        {
            DiscordLogger.LogCritical(GuildId, "Could not find the server for team ARKAD");
        }


        const ulong generalChannelId = 1384154960496820285;
        const ulong ssbuChannelId = 1205907392110534676;
        const ulong valorantChannelId = 1205907130339557486;
        // const ulong rocketLeagueChannelId = 1278311752509030540;
        const ulong arkadChannelId = 1027560985445273670;

        SocketGuildChannel? generalChannel = arkadGuild.GetTextChannel(generalChannelId);
        SocketGuildChannel? ssbuChannel = arkadGuild.GetTextChannel(ssbuChannelId);
        SocketGuildChannel? valorantChannel = arkadGuild.GetTextChannel(valorantChannelId);
        // SocketGuildChannel? rocketLeagueChannel = arkadGuild.GetTextChannel(rocketLeagueChannelId);
        SocketGuildChannel? arkadChannel = arkadGuild.GetTextChannel(arkadChannelId);

        SocketGuildChannel?[] channels =
            [generalChannel, ssbuChannel, valorantChannel/*, rocketLeagueChannel*/, arkadChannel];


        if (channels.Any(channel => channel is not SocketVoiceChannel))
        {
            DiscordLogger.LogCritical(GuildId,
                "Could not find the channels for team ARKAD or they were not voice channels.");
            return Task.CompletedTask;
        }

        // ChannelMaker generalMarker = new ((generalChannel as SocketVoiceChannel)!, client);
        // ChannelMaker ssbuMaker = new(ssbuChannel as SocketVoiceChannel, client);
        // ChannelMaker valorantMarker = new(valorantChannel as SocketVoiceChannel, client);
        // ChannelMaker rocketLeagueMarker = new(rocketLeagueChannel as SocketVoiceChannel, client);
        // ChannelMaker arkadLeagueMarker = new(arkadChannel as SocketVoiceChannel, client);

        Client.UserVoiceStateUpdated += (user, before, after) =>
        {
            if (user is not SocketGuildUser guildUser)
                return Task.CompletedTask;

            const string channelMarker = "'s Channel";
            if (guildUser.VoiceChannel is null && before.VoiceChannel.ConnectedUsers.Count == 0 &&
                before.VoiceChannel.Name.EndsWith(channelMarker))
            {
                Console.WriteLine("Deleting channel");
                before.VoiceChannel.DeleteAsync();
                DiscordLogger.Log(LogLevel.Information,
                    "Deleted {Channel} because it was empty as a result of {User} leaving (leaving to somewhere else not on the server or just disconnecting)",
                    before.VoiceChannel.Name, guildUser.Username);
                return Task.CompletedTask;
            }

            if (before.VoiceChannel is not null && before.VoiceChannel.ConnectedUsers.Count == 0 &&
                before.VoiceChannel.Name.EndsWith(channelMarker))
            {
                Console.WriteLine("Deleting channel");
                before.VoiceChannel.DeleteAsync();
                if (after.VoiceChannel is null)
                {
                    DiscordLogger.Log(arkadGuild, LogLevel.Information,
                        "Deleted {Channel} because it was empty as a result of {User} leaving (leaving to somewhere else not on the server or just disconnecting)",
                        before.VoiceChannel.Name, guildUser.Username);
                }
                else
                {
                    DiscordLogger.Log(arkadGuild, LogLevel.Information,
                        "Deleted {BeforeChannel} because it was empty as a result of {User} leaving (leaving to {AfterChannel})",
                        before.VoiceChannel.Name, guildUser.Username, after.VoiceChannel.Name);
                }
            }

            if (guildUser.VoiceChannel is null)
                return Task.CompletedTask;

            if (after.VoiceChannel is not null && after.VoiceChannel.Name.StartsWith("🔊") &&
                channels.Any(channel => channel is not null && channel.Id == after.VoiceChannel.Id) &&
                after.VoiceChannel.Id == guildUser.VoiceChannel.Id)
            {
                DiscordLogger.Log(arkadGuild, LogLevel.Information,
                    "{User} created {Channel}, as a result of joining the {TriggerChannel} channel", guildUser.Username,
                    guildUser.Username + channelMarker, after.VoiceChannel.Name);
                Task<IVoiceChannel> voiceChannel = after.VoiceChannel.Category.Guild.CreateVoiceChannelAsync(
                    guildUser.Username + channelMarker,
                    x => x.CategoryId = after.VoiceChannel.Category.Id);

                voiceChannel.Wait();
                return guildUser.ModifyAsync(props => props.Channel = new Optional<IVoiceChannel>(voiceChannel.Result));
            }

            return Task.CompletedTask;
        };

        Client.SlashCommandExecuted += command =>
        {
            const string introduction = "the parameters\n";
            StringBuilder builder = new(introduction);

            foreach (IApplicationCommandInteractionDataOption option in command.Data.Options)
            {
                builder.Append($"{option.Name}: {option.Value}\n");
            }

            if (builder.Length == introduction.Length)
            {
                builder.Clear();
                builder.Append("no parameters");
            }

            DiscordLogger.Log(LogLevel.Information,
                "{User} executed the slash command {Command} in {Channel} with {Parameters}",
                command.User.Username, command.CommandName, command.Channel, builder.ToString());

            return Task.CompletedTask;
        };

        Client.RoleCreated += role => ClientOnRoleCreated(arkadGuild, role);
        Client.RoleUpdated += (roleBefore, roleAfter) => ClientOnRoleUpdated(arkadGuild, roleBefore, roleAfter);
        Client.RoleDeleted += role => ClientOnRoleDeleted(arkadGuild, role);

        Client.UserUpdated += (userBefore, userAfter) => ClientOnUserUpdated(arkadGuild, userBefore, userAfter);

        return Task.CompletedTask;
    }

    private Task ClientOnUserUpdated(SocketGuild guild, SocketUser userBefore, SocketUser userAfter)
    {
        return Task.CompletedTask;
    }

    private Task ClientOnRoleUpdated(SocketGuild guild, SocketRole previousRole, SocketRole nextRole)
    {
        if (nextRole.Permissions.Administrator)
        {
            Task adminAcquiredWarning = ClientRoleCrudWarning(guild, nextRole, "updated");
            adminAcquiredWarning.Wait();
        }

        //Embed embed = BuildRoleUpdateEmbed(previousRole, nextRole);
        //DiscordLogger.GetLogger(guild)?.SendMessageAsync(embed: embed).Wait();

        return Task.CompletedTask;
    }

    /**private Embed BuildRoleUpdateEmbed(SocketRole previousRole, SocketRole nextRole)
    {
        SocketRole? truePreviousRole = RoleAssociationService.LookupRole(previousRole.Id);
        List<(string permission, PermissionState before, PermissionState after)> permissions = [];
        if (truePreviousRole is null)
        {
            DiscordLogger.LogCritical(GuildId, "Could not find the role {Role} in the database", previousRole.Name);
            return new EmbedBuilder().WithTitle("Role Updated")
                .WithDescription("Could not find the role in the database").Build();
        }

        ulong previousPermissions = truePreviousRole.Permissions.RawValue;
        ulong nextPermissions = nextRole.Permissions.RawValue;


        ulong flagCursor = 0;

        for (int i = 0; i < 64; i++)
        {
            if ((previousPermissions & flagCursor) != 0 && (nextPermissions & flagCursor) == 0)
            {
                permissions.Add((permissionNames[flagCursor], PermissionState.Allowed, PermissionState.Denied));
            }
            else if ((previousPermissions & flagCursor) == 0 && (nextPermissions & flagCursor) != 0)
            {
                permissions.Add((permissionNames[flagCursor], PermissionState.Denied, PermissionState.Allowed));
            }

            flagCursor <<= 1;
        }

        EmbedBuilder embedBuilder = new()
        {
            Title = "Role Updated",
            Description =
                previousRole.Name == nextRole.Name
                    ? $"**{previousRole.Name}** role update"
                    : $"**{previousRole.Name}** was updated to **{nextRole.Name}**",
            Color = Color.Blue,
            Timestamp = DateTimeOffset.UtcNow,
            Footer = new EmbedFooterBuilder { Text = $"ID: {nextRole.Id}" }
        };

        foreach ((string permission, PermissionState before, PermissionState after) in permissions)
        {
            embedBuilder.AddField($"Permission {permission} updated",
                $"{before.ToPermissionEmote()} => {after.ToPermissionEmote()}");
        }

        return embedBuilder.Build();
    }*/

    private Task ClientOnRoleCreated(SocketGuild guild, SocketRole role)
    {
        //RoleAssociationService.CreateRole(role);
        return ClientRoleCrudWarning(guild, role, "created");
    }

    private Task ClientOnRoleDeleted(SocketGuild guild, SocketRole role)
    {
        //RoleAssociationService.DeleteRole(role);
        return ClientRoleCrudWarning(guild, role, "deleted");
    }

    private Task ClientRoleCrudWarning(SocketGuild guild, SocketRole role, string action)
    {
        if (role.Permissions.Administrator)
        {
            DiscordLogger.Log(guild, LogLevel.Critical,
                "{User} {Action} the role {Role}", role.Guild.GetUser(role.Id).Username, action, role.Name);

            Task<RestUserMessage>? messageAsync = DiscordLogger.GetLogger(guild)?.SendMessageAsync(
                "A role with administrator permissions was updated. @everyone",
                allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone));

            messageAsync?.Wait();
        }
        else
        {
            DiscordLogger.Log(guild, LogLevel.Warning,
                "{User} {Action} the role {Role}", role.Guild.GetUser(role.Id).Username, action, role.Name);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        DiscordLogger.LogInformation(GuildId, "My Custom Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0); // Stop the timer
        Client.Ready -= OnClientReady; // Unsubscribe from the event
        return Task.CompletedTask;
    }
}

enum PermissionState
{
    Allowed,
    Inherited,
    Denied
}

internal static class PermissionExtensions
{
    public static string ToPermissionEmote(this PermissionState state) => state switch
    {
        PermissionState.Allowed => "white_check_mark",
        PermissionState.Inherited => "radio_button",
        PermissionState.Denied => "x",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
