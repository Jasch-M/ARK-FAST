using System.Reflection;
using ArkaZilla.Modals.GameStudios;
using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace ArkaZilla.Services;

public class InteractionHandler(
    DiscordSocketClient client,
    ILogger<InteractionHandler> logger,
    IServiceProvider provider,
    InteractionService service,
    IHostEnvironment environment,
    IOptions<StartupOptions> options
) : DiscordClientService(client, logger)
{
    internal static DiscordSocketClient? Bot { get; private set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.Ready += RegisterCommandsAsync;

        Client.Ready += async () =>
        {
            Bot = Client;
            await Bot.DownloadUsersAsync(Bot.Guilds);

            ModalService.Initialize();
            ApplicationsHandler.Instance.Initialize();

            Client.ModalSubmitted += ModalService.HandleModal;
            Client.ButtonExecuted += ModalService.HandleButton;
        };

        Client.InteractionCreated += OnInteractionCreated;

        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        await service.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);

        service.InteractionExecuted += OnInteractionExecuted;
    }

    private async Task RegisterCommandsAsync()
    {
        Logger.LogInformation("Registering commands...");

        if (environment.IsDevelopment())
            await RegisterCommandsLocallyAsync();
        else
            await RegisterCommandsGloballyAsync();

        Logger.LogInformation("Commands have been registered!");
    }

    private async Task RegisterCommandsLocallyAsync()
    {
        await Client.Rest.DeleteAllGlobalCommandsAsync();
        await service.RegisterCommandsToGuildAsync(options.Value.DevGuildId);
    }

    private async Task RegisterCommandsGloballyAsync()
    {

        await Client.Rest.DeleteAllGlobalCommandsAsync();

        for (int i = 0; i < Client.Guilds.Count; i++)
        {
            SocketGuild socketGuild = Client.Guilds.ElementAt(i);

            await socketGuild.DeleteApplicationCommandsAsync();
            await service.RegisterCommandsToGuildAsync(socketGuild.Id);
        }
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(Client, interaction);
            await service.ExecuteCommandAsync(context, provider);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Exception occurred whilst attempting to handle interaction.");
        }
    }

    private async Task OnInteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (result.Error == InteractionCommandError.UnknownCommand)
        {
            return;
        }

        if (string.IsNullOrEmpty(result.ErrorReason))
            return;

        await context.Interaction.HandleWithResultAsync(result);
    }
}
