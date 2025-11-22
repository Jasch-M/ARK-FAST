using ArkaZilla.Extensions.Logging;
using ArkaZilla.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace ArkaZilla.Modules;
/**
public class RoleAssociations : InteractionModuleBase<SocketInteractionContext>
{
    private readonly RoleAssociationService _roleAssociationService;
    private readonly DiscordBotUserService _botUserService;
    private readonly IOptions<ReferenceOptions> _options;

    private const string AssociateRolesConfirmationModal = "ASSOCIATE-ROLES-CONFIRMATION-MODAL";
    private const string YesConfirmationButton = "YES-CONFIRMATION-BUTTON";
    private const string NoConfirmationButton = "NO-CONFIRMATION-BUTTON";

    private const string YesConfirmationButtonResponse = "YES";
    private const string NoConfirmationButtonResponse = "NO";

    public RoleAssociations(DiscordBotUserService botUserService,
        IOptions<ReferenceOptions> options)
    {
        _roleAssociationService = botUserService.RoleAssociationService;
        _botUserService = botUserService;
        _options = options;

        _botUserService.Client.ModalSubmitted += modal =>
        {
            string modalId = modal.Data.CustomId;
            if (modalId != AssociateRolesConfirmationModal)
            {
                return Task.CompletedTask;
            }

            List<SocketMessageComponentData> socketMessageComponentDatas = modal.Data.Components.ToList();
            SocketMessageComponentData? yesButton = socketMessageComponentDatas.Find(component =>
                component.CustomId == YesConfirmationButton);
            SocketMessageComponentData? noButton = socketMessageComponentDatas.Find(component =>
                component.CustomId == NoConfirmationButton);

            if (yesButton is null)
            {
                _botUserService.Logger.LogError("Yes button not found in modal");
                modal.Message.Channel.SendMessageAsync("An Error Occured! I've logged the error for the developer :(", flags: MessageFlags.Ephemeral);
                return Task.CompletedTask;
            }

            if (noButton is null)
            {
                _botUserService.Logger.LogError("No button not found in modal");
                modal.Message.Channel.SendMessageAsync("An Error Occured! I've logged the error for the developer :(", flags: MessageFlags.Ephemeral);
                return Task.CompletedTask;
            }

            if (yesButton.Value != YesConfirmationButton)
            {
                _botUserService.Logger.LogError("Yes button has invalid value (\"{Value}\")", yesButton.Value);
                modal.Message.Channel.SendMessageAsync("An Error Occured! I've logged the error for the developer :(", flags: MessageFlags.Ephemeral);
                return Task.CompletedTask;
            }

            if (noButton.Value != NoConfirmationButton)
            {
                _botUserService.Logger.LogError("No button has invalid value (\"{Value}\")", noButton.Value);
                modal.Message.Channel.SendMessageAsync("An Error Occured! I've logged the error for the developer :(", flags: MessageFlags.Ephemeral);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        };
    }

    [SlashCommand("associate-roles", "Associate two roles into a same role group. " +
                                     "May merge two distinct role groups")]
    [RequireContext(contexts: ContextType.Guild)]
    [IsAuthorizedAdminUserService]
    public async Task AssociateRolesAsync(
        [Summary("first-role", "The first role to associate"),
         Autocomplete(typeof(RoleAssociationService.BaseRoleAutocompleteHandler))]
        [RoleAssociationService.BaseRolePrecondition]
        string firstRoleStr,
        [Summary("second-role", "The second role to associate"),
         Autocomplete(typeof(RoleAssociationService.BaseRoleAutocompleteHandler))]
        [RoleAssociationService.BaseRolePrecondition]
        string secondRoleStr)
    {
        ulong firstRole = ulong.Parse(firstRoleStr);
        ulong secondRole = ulong.Parse(secondRoleStr);

        SocketRole? firstRoleObject = _roleAssociationService.LookupRole(firstRole);
        SocketRole? secondRoleObject = _roleAssociationService.LookupRole(secondRole);

        if (firstRoleObject is null)
        {
            DiscordLogger.LogError("First role {RoleId} not found", firstRole);
            await RespondAsync(embed: new EmbedBuilder().WithColor(Color.Red).WithDescription("First role not found")
                .Build());
            return;
        }

        if (secondRoleObject is null)
        {
            DiscordLogger.LogError("Second role {RoleId} not found", secondRole);
            await RespondAsync(embed: new EmbedBuilder().WithColor(Color.Red).WithDescription("Second role not found")
                .Build());
            return;
        }

        (bool requiresMerge, HashSet<SocketRole> firstRoleSet, HashSet<SocketRole> secondRoleSet) mergeInformation =
            _roleAssociationService.RequiresMerge(firstRoleObject, secondRoleObject);

        if (mergeInformation.requiresMerge)
        {
            Modal confirmationModal = GenerateMergeModal(Context, firstRoleObject, secondRoleObject,
                mergeInformation.firstRoleSet, mergeInformation.secondRoleSet);
            await Context.Interaction.RespondWithModalAsync(confirmationModal);
            return;
        }

        await Context.Interaction.DeferAsync(true);
        bool added = _roleAssociationService.AddRoleAssociation(firstRoleObject, secondRoleObject);
        if (added)
        {
            Embed addedEmbed = new EmbedBuilder()
                .WithTitle($"{firstRoleObject.Name} ({firstRoleObject.Guild.Name}) :check: :chain: :check: {secondRoleObject.Name} ({secondRoleObject.Guild.Name})")
                .WithFields(new EmbedFieldBuilder()
                    .WithValue($"Merged the two roles into a role group with {Math.Max(mergeInformation.firstRoleSet.Count, mergeInformation.secondRoleSet.Count)} roles)")
                )
                .WithColor(Color.Green)
                .Build();

             await RespondAsync(embed: addedEmbed);
        }
    }

    [SlashCommand("create-role-group", "Create a role group from a role.")]
    [RequireContext(contexts: ContextType.Guild)]
    [IsAuthorizedAdminUserService]
    public async Task CreateRoleGroupAsync(
        [Summary("role", "The role to add as the first role in the role group.")] IRole role)
    { }

    private Modal GenerateMergeModal(SocketInteractionContext context, SocketRole firstRoleObject,
        SocketRole secondRoleObject, HashSet<SocketRole> firstRoleSet, HashSet<SocketRole> secondRoleSet)
    {
        EmbedBuilder warningEmbed = new EmbedBuilder { Title = ":warning: ROLE MERGE :warning:", }.AddField(
                $"{firstRoleObject.Name} ({firstRoleObject.Guild.Name}) {secondRoleObject.Name} ({secondRoleObject.Guild.Name}) Merge",
                "You are about to merge two previously distinct role groups into a single " +
                "role group!")
            .AddField("The first role group:\n",
                string.Join("\n",
                    firstRoleSet.Select(role => role.Id == firstRoleObject.Id
                        ? $"**{role.Name} ({role.Guild.Name})**"
                        : $"{role.Name} ({role.Guild.Name}")))
            .AddField("The second role group:\n",
                string.Join("\n",
                    secondRoleSet.Select(role => role.Id == secondRoleObject.Id
                        ? $"**{role.Name} ({role.Guild.Name})**"
                        : $"{role.Name} ({role.Guild.Name})")));

        context.Channel.SendMessageAsync(embed: warningEmbed.Build()).Wait();

        ButtonComponent yesButton =
            new ButtonBuilder(label: "PROCEED", customId: YesConfirmationButton, style: ButtonStyle.Success).Build();
        ButtonComponent noButton =
            new ButtonBuilder(label: "CANCEL", customId: NoConfirmationButton, style: ButtonStyle.Success).Build();

        ModalBuilder modalBuilder = new();

        modalBuilder.WithTitle("Are you sure you want to merge these role groups?");
        modalBuilder.WithCustomId(AssociateRolesConfirmationModal);
        modalBuilder.AddComponents([yesButton, noButton], 1);

        return modalBuilder.Build();
    }
}*/
