using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ArkaZilla.Services;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace ArkaZilla.Modals.GameStudios;

public class ApplicationsHandler : IModalHandler
{
    internal static readonly ApplicationsHandler Instance = new();

    public string ModalId => "arkad-game-studios-applying-form";

    private HashSet<SocketGuildUser> Members { get; set; } = [];

    private Dictionary<SocketUser, SocketTextChannel> Applicants { get; } = [];

    private Dictionary<SocketUser, ApplicationTimeout> ApplicationTimeouts { get; } = [];

    internal const string ApplicationChannelMarker = "'s application";

    internal const ulong ArkadGameStudiosId = 1393706299342065674;

    internal const ulong DevGuildId = 1393697222528471121;

    internal const ulong DevGuildApplicationCategoryId = 1402807291811467404;

    internal const ulong RecruiterRoleId = 1404384308957024286;

    internal const string RecruiterRoleName = "RECRUTER";

    internal const ulong ApplicantRoleId = 1404384596283363420;

    internal const string ApplicantRoleName = "APPLICANT";

    private ApplicationsHandler()
    {
        SocketGuild guild = InteractionHandler.Bot?.GetGuild(DevGuildId) ??
                            throw new InvalidOperationException("Bot is null");

        SocketCategoryChannel category = guild.GetCategoryChannel(DevGuildApplicationCategoryId);

        try
        {
            guild.DownloadUsersAsync().Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        foreach (var channel in category.Channels)
        {
            if (!channel.Name.EndsWith(ApplicationChannelMarker))
            {
                continue;
            }

            if (channel is not SocketTextChannel textChannel)
            {
                continue;
            }

            string username = channel.Name.Substring(0, channel.Name.Length - ApplicationChannelMarker.Length);
            ulong userId = InteractionHandler.Bot.GetUser(username).Id;

            SocketGuildUser socketGuildUser = guild.GetUser(userId);

            Applicants.Add(socketGuildUser, textChannel);
        }

        foreach (SocketGuildUser socketGuildUser in guild.Users)
        {
            if (socketGuildUser.Roles.Any(role => role.Id == ArkadGameStudiosId))
            {
                Members.Add(socketGuildUser);
            }
        }

        LoadTimeouts();
    }

    internal async Task AcceptUser(SocketGuildUser applicant)
    {
        if (!CheckUserIsApplicant(applicant, out SocketRole? applicantRole))
        {
            return;
        }

        SocketRole? arkadGameStudiosRole = applicant.Guild.GetRole(ArkadGameStudiosId);
        if (arkadGameStudiosRole is null)
        {
            return;
        }

        await applicant.RemoveRoleAsync(applicantRole);
        await applicant.AddRoleAsync(arkadGameStudiosRole);

        SocketTextChannel channel = Applicants[applicant];
        await channel.DeleteAsync();

        Applicants.Remove(applicant);
        Members.Add(applicant);
    }

    internal async Task DenyUser(SocketGuildUser applicant, DateTimeOffset expiration, string userFacingNote,
        string privateComment = "")
    {
        if (!CheckUserIsApplicant(applicant, out SocketRole? applicantRole))
        {
            return;
        }

        await applicant.RemoveRoleAsync(applicantRole);
        await applicant.SendMessageAsync(
            $"Thanks for applying to ARKAD Game Studios.\n" +
            $"Unfortunately your application to ARKAD Game Studios has been denied.\n" +
            $"{(string.IsNullOrEmpty(userFacingNote) ? "" : $"This comment on the denial is attached {userFacingNote}.")}\n" +
            $"You will be able to reapply {expiration.DateTime.ToLongDateString()} at {expiration.DateTime.ToLongTimeString()}.");

        Applicants.Remove(applicant);
        ApplicationTimeouts.Add(applicant, ApplicationTimeout.AfterDenial(expiration, userFacingNote, privateComment));
        SaveTimeouts();
    }

    internal async Task BlockUser(SocketGuildUser applicant, DateTimeOffset expiration, string userFacingNote,
        string privateComment = "")
    {
        if (!CheckUserIsApplicant(applicant, out SocketRole? applicantRole))
        {
            return;
        }

        await applicant.RemoveRoleAsync(applicantRole);
        await applicant.SendMessageAsync(
            $"A staff member has manually bared you from applying to ARKAD Game Studios\n" +
            $"Unfortunately your application to ARKAD Game Studios is bloked.\n" +
            $"{(string.IsNullOrEmpty(userFacingNote) ? "" : $"This comment on the denial is attached {userFacingNote}.")}\n" +
            $"You will be able to reapply {expiration.DateTime.ToLongDateString()} at {expiration.DateTime.ToLongTimeString()}.");

        Applicants.Remove(applicant);
        ApplicationTimeouts.Add(applicant, ApplicationTimeout.ManualBlock(expiration, userFacingNote, privateComment));
        SaveTimeouts();
    }

    private bool CheckUserIsApplicant(SocketGuildUser applicant, out SocketRole? applicantRole)
    {
        applicantRole = null;
        if (applicant.Guild.Id != DevGuildId)
        {
            return false; // TODO: Proper logging
        }

        if (applicant.Roles.Any(role => role.Name == "ARKAD Game Studios"))
        {
            return false;
        }

        SocketRole recruiterRole = applicant.Guild.GetRole(RecruiterRoleId);
        applicantRole = applicant.Guild.GetRole(ApplicantRoleId);

        if (recruiterRole is null)
        {
            return false;
        }

        if (applicantRole is null)
        {
            return false;
        }

        if (applicant.Roles.Any(role => role.Id == RecruiterRoleId))
        {
            return false;
        }

        if (applicant.Roles.Any(role => role.Id == ApplicantRoleId))
        {
            return true;
        }

        return false;
    }

    public async Task HandleModal(SocketModal modal)
    {
        const string arkCuteString = "<:ARKCute:1236821760263848046>";

        if (InteractionHandler.Bot is null)
        {
            Console.WriteLine("Critical: bot value is null this should never happen"); //TODO: Make this proper logging
            return;
        }

        Embed applicationEmbed = GenerateApplicationEmbed(modal);

        if (Members.Contains(modal.User))
        {
            await modal.RespondAsync(
                $"{arkCuteString}\nHey that's super kind of you wanting to apply a second time.\n" +
                "The thing is...you're already a member of ARKAD Game Studios so you can't " +
                "apply again. Though I sent you your application as a DM should you need it",
                ephemeral: true);
            await modal.User.SendMessageAsync(embed: applicationEmbed);
            return;
        }
        else if (Applicants.ContainsKey(modal.User))
        {
            await modal.RespondAsync($"{arkCuteString}\nYou already have an application open...you can only have one." +
                                     "I have however sent you a DM with your application should you want to use it\n",
                ephemeral: true);
            await modal.User.SendMessageAsync(embed: applicationEmbed);
            return;
        }
        else if (ApplicationTimeouts.ContainsKey(modal.User))
        {
            string message = ApplicationTimeouts[modal.User].Reason switch
            {
                ApplicationTimeout.ApplicationTimeoutReason.AfterDenial =>
                    $"Your previous application was rejected and you will be able to reapply " +
                    $"{new TimestampTag(ApplicationTimeouts[modal.User].Expiration)}" +
                    $"{(string.IsNullOrWhiteSpace(ApplicationTimeouts[modal.User].UserFacingNote)
                        ? "."
                        : $" with the following attached:\n\"{ApplicationTimeouts[modal.User].UserFacingNote}\".")}",
                ApplicationTimeout.ApplicationTimeoutReason.ManualBlock =>
                    "You have been blocked from applying to ARKAD Game Studios for the following reason:\n" +
                    $"{ApplicationTimeouts[modal.User].UserFacingNote}\n" +
                    "If you feel this is in error, please contact a staff member.\n" +
                    "The expiry for the block will be " +
                    $"{new TimestampTag(ApplicationTimeouts[modal.User].Expiration)}",
                _ => throw new UnreachableException(
                    "Something went horribly wrong where the reason was allowed to be the integer value" +
                    $"{(int)ApplicationTimeouts[modal.User].Reason} which is not a well defined enum case")
            };

            message += "\n\n" +
                       "I have sent to you a DM with your application should you want to use it\n";
            await modal.RespondAsync(message, ephemeral: true);
            await modal.User.SendMessageAsync(embed: applicationEmbed);
        }

        await modal.RespondAsync("Excellent excellent, an enrollment channel will be open just for you",
            ephemeral: true);


        SocketGuild? guild = InteractionHandler.Bot.GetGuild(DevGuildId);
        if (guild is null)
        {
            return;
        }

        SocketCategoryChannel? category = guild.GetCategoryChannel(DevGuildApplicationCategoryId);
        if (category is null)
        {
            await modal.User.SendMessageAsync("Something went wrong. I have contacted the developers about the issue." +
                                              "we will get back to you as soon as possible.");
            return;
        }

        List<Overwrite> permissionOverwrites = GetPermissionOverwrites(modal.User);
        SocketRole? applicantRole = guild.GetRole(ApplicantRoleId);
        if (applicantRole is null)
        {
            await modal.User.SendMessageAsync("Something went wrong. I have contacted the developers about the issue." +
                                              "we will get back to you as soon as possible.");
            return;
        }

        SocketGuildUser? applicant = guild.GetUser(modal.User.Id);
        if (applicant is null)
        {
            await modal.User.SendMessageAsync("Something went wrong. I have contacted the developers about the issue." +
                                              "we will get back to you as soon as possible.");
            return;
        }

        await applicant.AddRoleAsync(applicantRole);

        RestTextChannel restChannel = await guild.CreateTextChannelAsync(
            $"{modal.User.Username}{ApplicationChannelMarker}", properties =>
            {
                properties.CategoryId = category.Id;
                properties.PermissionOverwrites = Optional.Create<IEnumerable<Overwrite>>(permissionOverwrites);
            });
        await restChannel.SendMessageAsync(embed: applicationEmbed);
        await restChannel.SendMessageAsync(
            $"{modal.User.Mention} discussion related to your application with the staff will happen in this channel");

        SocketTextChannel channel = guild.GetTextChannel(restChannel.Id);

        Applicants.Add(modal.User, channel);
    }

    private static Embed GenerateApplicationEmbed(SocketModal modal)
    {
        IReadOnlyCollection<SocketMessageComponentData> socketMessageComponents = modal.Data.Components;

        if (socketMessageComponents.Count != 4)
        {
            return GenerateErrorApplicationEmbed();
        }


        SocketMessageComponentData whyArkad;
        SocketMessageComponentData offer;
        SocketMessageComponentData experience;
        SocketMessageComponentData specialNeeds;

        try
        {
            whyArkad =
                socketMessageComponents.First(message => message.CustomId == ApplyingHandler.WhyArkadId);
            offer = socketMessageComponents.First(message => message.CustomId == ApplyingHandler.OfferId);
            experience = socketMessageComponents.First(message => message.CustomId == ApplyingHandler.ExperienceId);
            specialNeeds = socketMessageComponents.First(message => message.CustomId == ApplyingHandler.SpecialNeedsId);
        }
        catch (InvalidOperationException _)
        {
            return GenerateErrorApplicationEmbed();
        }

        EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle($"{modal.User.Mention}'s Application")
            .AddField(new EmbedFieldBuilder()
                .WithName(ApplyingHandler.WhyArkadQuestion)
                .WithValue(whyArkad.Value))
            .AddField(new EmbedFieldBuilder()
                .WithName(ApplyingHandler.OfferQuestion)
                .WithValue(offer.Value));

        if (!string.IsNullOrWhiteSpace(experience.Value))
        {
            embedBuilder.AddField(new EmbedFieldBuilder()
                .WithName(ApplyingHandler.ExperienceQuestion)
                .WithValue(experience.Value));
        }

        if (!string.IsNullOrWhiteSpace(specialNeeds.Value))
        {
            embedBuilder.AddField(new EmbedFieldBuilder()
                .WithName(ApplyingHandler.SpecialNeedsQuestion)
                .WithValue(specialNeeds.Value));
        }

        return embedBuilder.Build();
    }

    private static Embed GenerateErrorApplicationEmbed()
    {
        return new EmbedBuilder().WithTitle("An error occured.")
            .WithDescription("I have informed the developers")
            .WithColor(Color.Red).Build();
    }

    private static List<Overwrite> GetPermissionOverwrites(SocketUser user)
    {
        const ulong directorRole = 1393719736625401866;
        const ulong accountableRole = 1393719736625401866;
        const ulong recruiterRole = 1404384308957024286;
        const ulong everyoneRole = 1393697222528471121;

        OverwritePermissions directorPermissions = new(
            viewChannel: PermValue.Allow, manageChannel: PermValue.Allow, manageWebhooks: PermValue.Allow,
            manageRoles: PermValue.Allow,
            createInstantInvite: PermValue.Deny, sendMessages: PermValue.Allow, sendMessagesInThreads: PermValue.Allow,
            createPublicThreads: PermValue.Allow, createPrivateThreads: PermValue.Allow, embedLinks: PermValue.Allow,
            attachFiles: PermValue.Allow, addReactions: PermValue.Allow, useExternalEmojis: PermValue.Allow,
            useExternalStickers: PermValue.Allow, mentionEveryone: PermValue.Allow, manageMessages: PermValue.Allow,
            manageThreads: PermValue.Allow, readMessageHistory: PermValue.Allow, sendTTSMessages: PermValue.Allow,
            sendVoiceMessages: PermValue.Allow, sendPolls: PermValue.Allow,
            useApplicationCommands: PermValue.Allow, useSlashCommands: PermValue.Allow,
            startEmbeddedActivities: PermValue.Allow,
            useExternalApps: PermValue.Allow,
            connect: PermValue.Allow, speak: PermValue.Allow, stream: PermValue.Allow,
            useSoundboard: PermValue.Allow, useExternalSounds: PermValue.Allow, useVoiceActivation: PermValue.Allow,
            prioritySpeaker: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow,
            moveMembers: PermValue.Allow, setVoiceChannelStatus: PermValue.Allow
        );

        OverwritePermissions accountablePermissions = directorPermissions;
        OverwritePermissions everyonePermissions = new(
            viewChannel: PermValue.Deny, manageChannel: PermValue.Deny, manageWebhooks: PermValue.Deny,
            manageRoles: PermValue.Deny,
            createInstantInvite: PermValue.Deny, sendMessages: PermValue.Deny, sendMessagesInThreads: PermValue.Deny,
            createPublicThreads: PermValue.Deny, createPrivateThreads: PermValue.Deny, embedLinks: PermValue.Deny,
            attachFiles: PermValue.Deny, addReactions: PermValue.Deny, useExternalEmojis: PermValue.Deny,
            useExternalStickers: PermValue.Deny, mentionEveryone: PermValue.Deny, manageMessages: PermValue.Deny,
            manageThreads: PermValue.Deny, readMessageHistory: PermValue.Deny, sendTTSMessages: PermValue.Deny,
            sendVoiceMessages: PermValue.Deny, sendPolls: PermValue.Deny,
            useApplicationCommands: PermValue.Deny, useSlashCommands: PermValue.Deny,
            startEmbeddedActivities: PermValue.Deny,
            useExternalApps: PermValue.Deny,
            connect: PermValue.Deny, speak: PermValue.Deny, stream: PermValue.Deny,
            useSoundboard: PermValue.Deny, useExternalSounds: PermValue.Deny, useVoiceActivation: PermValue.Deny,
            prioritySpeaker: PermValue.Deny, muteMembers: PermValue.Deny, deafenMembers: PermValue.Deny,
            moveMembers: PermValue.Deny, setVoiceChannelStatus: PermValue.Deny);

        OverwritePermissions recruiterPermissions = accountablePermissions.Modify(manageChannel: PermValue.Deny,
            manageWebhooks: PermValue.Deny, manageRoles: PermValue.Deny, createPrivateThreads: PermValue.Deny,
            manageMessages: PermValue.Deny, manageThreads: PermValue.Deny, sendPolls: PermValue.Deny,
            startEmbeddedActivities: PermValue.Deny, useExternalApps: PermValue.Deny);

        OverwritePermissions userPermissions = recruiterPermissions.Modify(createPublicThreads: PermValue.Deny,
            mentionEveryone: PermValue.Deny, useApplicationCommands: PermValue.Deny, useSlashCommands: PermValue.Deny);

        Overwrite directorOverwrite = new(directorRole, PermissionTarget.Role, directorPermissions);
        Overwrite accountableOverwrite = new(accountableRole, PermissionTarget.Role, accountablePermissions);
        Overwrite recruiterOverwrite = new(recruiterRole, PermissionTarget.Role, recruiterPermissions);
        Overwrite everyoneOverwrite = new(everyoneRole, PermissionTarget.Role, everyonePermissions);
        Overwrite userOverwrite = new(user.Id, PermissionTarget.User, userPermissions);

        return [directorOverwrite, recruiterOverwrite, accountableOverwrite, everyoneOverwrite, userOverwrite];
    }

    public class ApplicationTimeout
    {
        public DateTimeOffset Expiration { get; }

        public ApplicationTimeoutReason Reason { get; }

        public string UserFacingNote { get; }

        public string PrivateComment { get; }

        private ApplicationTimeout(DateTimeOffset expiration, ApplicationTimeoutReason reason, string userFacingNote,
            string privateComment)
        {
            Expiration = expiration;
            Reason = reason;
            UserFacingNote = userFacingNote;
            PrivateComment = privateComment;
        }

        internal static ApplicationTimeout AfterDenial(DateTimeOffset expiration, string userFacingNote = "",
            string privateComment = "") =>
            new(expiration, ApplicationTimeoutReason.AfterDenial, userFacingNote, privateComment);

        internal static ApplicationTimeout ManualBlock(DateTimeOffset expiration, string userFacingNote = "",
            string privateComment = "") =>
            new(expiration, ApplicationTimeoutReason.ManualBlock, userFacingNote, privateComment);


        public enum ApplicationTimeoutReason
        {
            AfterDenial,
            ManualBlock
        }

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                ApplicationTimeout timeout => timeout.Expiration == Expiration && timeout.Reason == Reason,
                ApplicationTimeoutReason reason => reason == Reason,
                _ => false
            };
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Expiration, Reason);
        }
    }

    private const string TimeoutsFile = "../../../Assets/Saves/blocked-users.json";

    private string LoadTimeouts()
    {
        if (!File.Exists(TimeoutsFile))
        {
            File.WriteAllText(TimeoutsFile, "[]");
            return "Timeout file not found, created a new one. Assuming no timeouts.";
        }

        string json;
        try
        {
            json = File.ReadAllText(TimeoutsFile);
        }
        catch (IOException e)
        {
            return $"Error reading timeout file: {e.Message}. Timeouts not loaded.";
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return "Timeout file is empty, no timeouts loaded.";
        }

        SocketGuild? guild = InteractionHandler.Bot?.GetGuild(1393697222528471121);
        if (guild is null)
        {
            return "Cannot find guild, timeouts not loaded.";
        }

        int loadedTimeouts = 0;
        int skippedEntries = 0;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return "Timeout file is not a valid JSON array. Timeouts not loaded.";
            }

            foreach (JsonElement element in doc.RootElement.EnumerateArray())
            {
                if (!element.TryGetProperty("id", out JsonElement idElement) ||
                    idElement.ValueKind != JsonValueKind.String ||
                    !ulong.TryParse(idElement.GetString(), out ulong userId))
                {
                    skippedEntries++;
                    continue;
                }

                SocketGuildUser? user = guild.GetUser(userId);
                if (user == null)
                {
                    skippedEntries++;
                    continue;
                }

                if (!element.TryGetProperty("timestamp", out JsonElement timestampElement) ||
                    !timestampElement.TryGetInt64(out long unixTimestamp))
                {
                    skippedEntries++;
                    continue;
                }

                DateTime expiration = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;

                if (!element.TryGetProperty("type", out JsonElement typeElement) ||
                    typeElement.ValueKind != JsonValueKind.String)
                {
                    skippedEntries++;
                    continue;
                }

                ApplicationTimeout.ApplicationTimeoutReason reason;
                switch (typeElement.GetString())
                {
                    case "denied":
                        reason = ApplicationTimeout.ApplicationTimeoutReason.AfterDenial;
                        break;
                    case "manual-block":
                        reason = ApplicationTimeout.ApplicationTimeoutReason.ManualBlock;
                        break;
                    default:
                        skippedEntries++;
                        continue;
                }

                if (!element.TryGetProperty("user-facing-note", out JsonElement userNoteElement) ||
                    userNoteElement.ValueKind != JsonValueKind.String)
                {
                    skippedEntries++;
                    continue;
                }

                string userFacingNote = userNoteElement.GetString() ?? string.Empty;

                if (!element.TryGetProperty("private-facing-note", out JsonElement privateNoteElement) ||
                    privateNoteElement.ValueKind != JsonValueKind.String)
                {
                    skippedEntries++;
                    continue;
                }

                string privateComment = privateNoteElement.GetString() ?? string.Empty;

                ApplicationTimeout timeout = reason switch
                {
                    ApplicationTimeout.ApplicationTimeoutReason.AfterDenial => ApplicationTimeout.AfterDenial(
                        expiration, userFacingNote, privateComment),
                    ApplicationTimeout.ApplicationTimeoutReason.ManualBlock => ApplicationTimeout.ManualBlock(
                        expiration, userFacingNote, privateComment),
                    _ => throw new InvalidOperationException("Unhandled reason in LoadTimeouts")
                };

                ApplicationTimeouts[user] = timeout;
                loadedTimeouts++;
            }
        }
        catch (JsonException jsonException)
        {
            return $"Error parsing timeout file: {jsonException.Message}. Timeouts not loaded.";
        }
        catch (Exception e)
        {
            return $"An unexpected error occured: {e.Message}. Timeouts not loaded.";
        }

        return $"Loaded {loadedTimeouts} application timeouts. Skipped {skippedEntries} invalid entries.";
    }

    private void SaveTimeouts()
    {
        const int indentationSpaces = 4;
        StringBuilder jsonBuilder = new();
        jsonBuilder.AppendLine("[");

        int indentation = indentationSpaces;

        int writtenUsers = 0;
        foreach (KeyValuePair<SocketUser, ApplicationTimeout> timeout in ApplicationTimeouts)
        {
            SocketUser user = timeout.Key;
            ApplicationTimeout applicationTimeout = timeout.Value;

            jsonBuilder.Append(' ', indentation);
            jsonBuilder.AppendLine("{");
            indentation += indentationSpaces;

            jsonBuilder.Append(' ', indentation);
            jsonBuilder.AppendLine("\"id\": \"" + user.Id + "\",");
            jsonBuilder.Append(' ', indentation);
            jsonBuilder.AppendLine("\"timestamp\": " + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ",");

            jsonBuilder.Append(' ', indentation);
            jsonBuilder.AppendLine("\"type\": \"" + applicationTimeout.Reason.ToJsonValue() + "\",");

            jsonBuilder.Append(' ', indentation);
            jsonBuilder.AppendLine("\"user-facing-note\": \"" + applicationTimeout.UserFacingNote + "\",");
            jsonBuilder.Append(' ', indentation);
            jsonBuilder.AppendLine("\"private-facing-note\": \"" + applicationTimeout.PrivateComment + "\"");

            indentation -= indentationSpaces;
            jsonBuilder.Append(' ', indentation);
            jsonBuilder.Append('}');

            writtenUsers++;

            if (writtenUsers < ApplicationTimeouts.Count)
            {
                jsonBuilder.AppendLine(",");
            }
            else
            {
                jsonBuilder.AppendLine();
            }
        }

        indentation -= indentationSpaces;
        jsonBuilder.Append(' ', indentation);
        jsonBuilder.Append(']');

        File.WriteAllText(TimeoutsFile, jsonBuilder.ToString());
    }
}

public static class ApplicationTimeoutReasonExtensions
{
    public static string ToJsonValue(this ApplicationsHandler.ApplicationTimeout.ApplicationTimeoutReason reason)
    {
        return reason switch
        {
            ApplicationsHandler.ApplicationTimeout.ApplicationTimeoutReason.AfterDenial => "denied",
            ApplicationsHandler.ApplicationTimeout.ApplicationTimeoutReason.ManualBlock => "manual-block",
            _ => throw new InvalidOperationException("Unhandled reason in ToJsonValue")
        };
    }
}
