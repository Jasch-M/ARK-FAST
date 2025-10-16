using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using ArkaZilla;
using ArkaZilla.Data;
using ArkaZilla.Services;

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddNamedOptions<StartupOptions>();
builder.Services.AddNamedOptions<ReferenceOptions>();

builder.Services.AddNpgsql<ArkaZillaDbContext>(builder.Configuration.GetConnectionString("Default"),
    options => options.MigrationsHistoryTable("__EFMigrationsHistory", "arkad").MigrationsAssembly(typeof(ArkaZillaDbContext).Assembly.FullName));

builder.Services.AddDiscordHost((config, _) =>
{
    config.SocketConfig = new DiscordSocketConfig
    {
        LogLevel = LogSeverity.Info,
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
        LogGatewayIntentWarnings = false,
        UseInteractionSnowflakeDate = false,
        AlwaysDownloadUsers = false,
    };

    config.Token = builder.Configuration.GetSection(StartupOptions.GetSectionName()).Get<StartupOptions>()!.Token;
});

builder.Services.AddInteractionService((config, _) =>
{
    config.LogLevel = LogSeverity.Debug;
    config.DefaultRunMode = RunMode.Async;
    config.UseCompiledLambda = true;
});

builder.Services.AddInteractiveService(config =>
{
    config.LogLevel = LogSeverity.Warning;
    config.DefaultTimeout = TimeSpan.FromMinutes(5);
    config.ProcessSinglePagePaginators = true;
});

// First register the concrete service implementation
builder.Services.AddSingleton<DiscordBotUserService>();
// Then register it as the interface implementation
builder.Services.AddSingleton<IDiscordBotUserService>(sp => sp.GetRequiredService<DiscordBotUserService>());
// Finally register it as a hosted service
builder.Services.AddHostedService(sp => sp.GetRequiredService<DiscordBotUserService>());

builder.Services.AddHostedService<InteractionHandler>();
var host = builder.Build();

await host.MigrateAsync<ArkaZillaDbContext>();
await host.RunAsync();
