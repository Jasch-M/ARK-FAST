using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace ArkaZilla.Extensions.Logging;

public class DiscordLogger(ILogger logger)
{
    public static DiscordLogger? Logger { get; internal set; }

    private readonly Dictionary<SocketGuild, SocketTextChannel> _loggers = new();

    private readonly Dictionary<ulong, SocketTextChannel> _loggerIds = new();

    private readonly ILogger _logger = logger;

    private static readonly Func<FormattedLogValues, Exception?, string> _messageFormatter = MessageFormatter;

    public static void LogTrace(string? message, params object?[] args)
    {
        Log(LogLevel.Trace, 0, null, message, args);
    }

    public static void LogTrace(SocketTextChannel loggingChannel, string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Trace, 0, null, message, args);
    }

    public static void LogTrace(SocketGuild guild, string? message, params object?[] args)
    {
        Log(guild, LogLevel.Trace, 0, null, message, args);
    }

    public static void LogTrace(ulong id, string? message, params object?[] args)
    {
        Log(id, LogLevel.Trace, 0, null, message, args);
    }

    public static void LogTrace(EventId eventId, string? message, params object?[] args)
    {
        Log(LogLevel.Trace, eventId, null, message, args);
    }

    public static void LogTrace(SocketTextChannel loggingChannel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(loggingChannel, LogLevel.Trace, eventId, null, message, args);
    }

    public static void LogTrace(SocketGuild guild, EventId eventId, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Trace, eventId, null, message, args);
    }

    public static void LogTrace(ulong id, EventId eventId, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Trace, eventId, null, message, args);
    }

    public static void LogTrace(Exception exception, string? message, params object?[] args)
    {
        Log(LogLevel.Trace, 0, exception, message, args);
    }

    public static void LogTrace(SocketTextChannel loggingChannel, Exception exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Trace, 0, exception, message, args);
    }

    public static void LogTrace(SocketGuild guild, Exception exception, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Trace, 0, exception, message, args);
    }

    public static void LogTrace(ulong id, Exception exception, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Trace, 0, exception, message, args);
    }

    public static void LogTrace(EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(LogLevel.Trace, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogTrace(SocketTextChannel loggingChannel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Trace, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogTrace(SocketGuild guild, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(guild, LogLevel.Trace, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogTrace(ulong id, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(id, LogLevel.Trace, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogTrace<TState>(EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(LogLevel.Trace, eventId, state, exception, formatter);
    }

    public static void LogTrace<TState>(SocketGuild guild, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(guild, LogLevel.Trace, eventId, state, exception, formatter);
    }

    public static void LogTrace<TState>(SocketTextChannel loggingChannel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Log(loggingChannel, LogLevel.Trace, eventId, state, exception, formatter);
    }

    public static void LogDebug(string? message, params object?[] args)
    {
        Log(LogLevel.Debug, 0, null, message, args);
    }

    public static void LogDebug(SocketTextChannel loggingChannel, string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Debug, 0, null, message, args);
    }

    public static void LogDebug(SocketGuild guild, string? message, params object?[] args)
    {
        Log(guild, LogLevel.Debug, 0, null, message, args);
    }

    public static void LogDebug(ulong id, string? message, params object?[] args)
    {
        Log(id, LogLevel.Debug, 0, null, message, args);
    }

    public static void LogDebug(EventId eventId, string? message, params object?[] args)
    {
        Log(LogLevel.Debug, eventId, null, message, args);
    }

    public static void LogDebug(SocketTextChannel loggingChannel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(loggingChannel, LogLevel.Debug, eventId, null, message, args);
    }

    public static void LogDebug(SocketGuild guild, EventId eventId, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Debug, eventId, null, message, args);
    }

    public static void LogDebug(ulong id, EventId eventId, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Debug, eventId, null, message, args);
    }

    public static void LogDebug(Exception exception, string? message, params object?[] args)
    {
        Log(LogLevel.Debug, 0, exception, message, args);
    }

    public static void LogDebug(SocketTextChannel loggingChannel, Exception exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Debug, 0, exception, message, args);
    }

    public static void LogDebug(SocketGuild guild, Exception exception, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Debug, 0, exception, message, args);
    }

    public static void LogDebug(ulong id, Exception exception, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Debug, 0, exception, message, args);
    }

    public static void LogDebug(EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(LogLevel.Debug, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogDebug(SocketTextChannel loggingChannel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Debug, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogDebug(SocketGuild guild, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(guild, LogLevel.Debug, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogDebug(ulong id, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(id, LogLevel.Debug, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogDebug<TState>(EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(LogLevel.Debug, eventId, state, exception, formatter);
    }

    public static void LogDebug<TState>(SocketGuild guild, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(guild, LogLevel.Debug, eventId, state, exception, formatter);
    }

    public static void LogDebug<TState>(SocketTextChannel loggingChannel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Log(loggingChannel, LogLevel.Debug, eventId, state, exception, formatter);
    }

    public static void LogInformation(string? message, params object?[] args)
    {
        Log(LogLevel.Information, 0, null, message, args);
    }

    public static void LogInformation(SocketTextChannel loggingChannel, string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Information, 0, null, message, args);
    }

    public static void LogInformation(SocketGuild guild, string? message, params object?[] args)
    {
        Log(guild, LogLevel.Information, 0, null, message, args);
    }

    public static void LogInformation(ulong id, string? message, params object?[] args)
    {
        Log(id, LogLevel.Information, 0, null, message, args);
    }

    public static void LogInformation(EventId eventId, string? message, params object?[] args)
    {
        Log(LogLevel.Information, eventId, null, message, args);
    }

    public static void LogInformation(SocketTextChannel loggingChannel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(loggingChannel, LogLevel.Information, eventId, null, message, args);
    }

    public static void LogInformation(SocketGuild guild, EventId eventId, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Information, eventId, null, message, args);
    }

    public static void LogInformation(ulong id, EventId eventId, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Information, eventId, null, message, args);
    }

    public static void LogInformation(Exception exception, string? message, params object?[] args)
    {
        Log(LogLevel.Information, 0, exception, message, args);
    }

    public static void LogInformation(SocketTextChannel loggingChannel, Exception exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Information, 0, exception, message, args);
    }

    public static void LogInformation(SocketGuild guild, Exception exception, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Information, 0, exception, message, args);
    }

    public static void LogInformation(ulong id, Exception exception, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Information, 0, exception, message, args);
    }

    public static void LogInformation(EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(LogLevel.Information, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogInformation(SocketTextChannel loggingChannel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Information, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogInformation(SocketGuild guild, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(guild, LogLevel.Information, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogInformation(ulong id, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(id, LogLevel.Information, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogInformation<TState>(EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(LogLevel.Information, eventId, state, exception, formatter);
    }

    public static void LogInformation<TState>(SocketGuild guild, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(guild, LogLevel.Information, eventId, state, exception, formatter);
    }

    public static void LogInformation<TState>(SocketTextChannel loggingChannel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Log(loggingChannel, LogLevel.Information, eventId, state, exception, formatter);
    }

    public static void LogWarning(string? message, params object?[] args)
    {
        Log(LogLevel.Warning, 0, null, message, args);
    }

    public static void LogWarning(SocketTextChannel loggingChannel, string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Warning, 0, null, message, args);
    }

    public static void LogWarning(SocketGuild guild, string? message, params object?[] args)
    {
        Log(guild, LogLevel.Warning, 0, null, message, args);
    }

    public static void LogWarning(ulong id, string? message, params object?[] args)
    {
        Log(id, LogLevel.Warning, 0, null, message, args);
    }

    public static void LogWarning(EventId eventId, string? message, params object?[] args)
    {
        Log(LogLevel.Warning, eventId, null, message, args);
    }

    public static void LogWarning(SocketTextChannel loggingChannel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(loggingChannel, LogLevel.Warning, eventId, null, message, args);
    }

    public static void LogWarning(SocketGuild guild, EventId eventId, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Warning, eventId, null, message, args);
    }

    public static void LogWarning(ulong id, EventId eventId, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Warning, eventId, null, message, args);
    }

    public static void LogWarning(Exception exception, string? message, params object?[] args)
    {
        Log(LogLevel.Warning, 0, exception, message, args);
    }

    public static void LogWarning(SocketTextChannel loggingChannel, Exception exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Warning, 0, exception, message, args);
    }

    public static void LogWarning(SocketGuild guild, Exception exception, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Warning, 0, exception, message, args);
    }

    public static void LogWarning(ulong id, Exception exception, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Warning, 0, exception, message, args);
    }

    public static void LogWarning(EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(LogLevel.Warning, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogWarning(SocketTextChannel loggingChannel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Warning, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogWarning(SocketGuild guild, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(guild, LogLevel.Warning, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogWarning(ulong id, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(id, LogLevel.Warning, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogWarning<TState>(EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(LogLevel.Warning, eventId, state, exception, formatter);
    }

    public static void LogWarning<TState>(SocketGuild guild, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(guild, LogLevel.Warning, eventId, state, exception, formatter);
    }

    public static void LogWarning<TState>(SocketTextChannel loggingChannel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Log(loggingChannel, LogLevel.Warning, eventId, state, exception, formatter);
    }

    public static void LogError(string? message, params object?[] args)
    {
        Log(LogLevel.Error, 0, null, message, args);
    }

    public static void LogError(SocketTextChannel loggingChannel, string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Error, 0, null, message, args);
    }

    public static void LogError(SocketGuild guild, string? message, params object?[] args)
    {
        Log(guild, LogLevel.Error, 0, null, message, args);
    }

    public static void LogError(ulong id, string? message, params object?[] args)
    {
        Log(id, LogLevel.Error, 0, null, message, args);
    }

    public static void LogError(EventId eventId, string? message, params object?[] args)
    {
        Log(LogLevel.Error, eventId, null, message, args);
    }

    public static void LogError(SocketTextChannel loggingChannel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(loggingChannel, LogLevel.Error, eventId, null, message, args);
    }

    public static void LogError(SocketGuild guild, EventId eventId, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Error, eventId, null, message, args);
    }

    public static void LogError(ulong id, EventId eventId, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Error, eventId, null, message, args);
    }

    public static void LogError(Exception exception, string? message, params object?[] args)
    {
        Log(LogLevel.Error, 0, exception, message, args);
    }

    public static void LogError(SocketTextChannel loggingChannel, Exception exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Error, 0, exception, message, args);
    }

    public static void LogError(SocketGuild guild, Exception exception, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Error, 0, exception, message, args);
    }

    public static void LogError(ulong id, Exception exception, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Error, 0, exception, message, args);
    }

    public static void LogError(EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(LogLevel.Error, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogError(SocketTextChannel loggingChannel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Error, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogError(SocketGuild guild, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(guild, LogLevel.Error, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogError(ulong id, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(id, LogLevel.Error, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogError<TState>(EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(LogLevel.Error, eventId, state, exception, formatter);
    }

    public static void LogError<TState>(SocketGuild guild, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(guild, LogLevel.Error, eventId, state, exception, formatter);
    }

    public static void LogError<TState>(SocketTextChannel loggingChannel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Log(loggingChannel, LogLevel.Error, eventId, state, exception, formatter);
    }

    public static void LogCritical(string? message, params object?[] args)
    {
        Log(LogLevel.Critical, 0, null, message, args);
    }

    public static void LogCritical(SocketTextChannel loggingChannel, string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Critical, 0, null, message, args);
    }

    public static void LogCritical(SocketGuild guild, string? message, params object?[] args)
    {
        Log(guild, LogLevel.Critical, 0, null, message, args);
    }

    public static void LogCritical(ulong id, string? message, params object?[] args)
    {
        Log(id, LogLevel.Critical, 0, null, message, args);
    }

    public static void LogCritical(EventId eventId, string? message, params object?[] args)
    {
        Log(LogLevel.Critical, eventId, null, message, args);
    }

    public static void LogCritical(SocketTextChannel loggingChannel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(loggingChannel, LogLevel.Critical, eventId, null, message, args);
    }

    public static void LogCritical(SocketGuild guild, EventId eventId, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Critical, eventId, null, message, args);
    }

    public static void LogCritical(ulong id, EventId eventId, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Critical, eventId, null, message, args);
    }

    public static void LogCritical(Exception exception, string? message, params object?[] args)
    {
        Log(LogLevel.Critical, 0, exception, message, args);
    }

    public static void LogCritical(SocketTextChannel loggingChannel, Exception exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Critical, 0, exception, message, args);
    }

    public static void LogCritical(SocketGuild guild, Exception exception, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.Critical, 0, exception, message, args);
    }

    public static void LogCritical(ulong id, Exception exception, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.Critical, 0, exception, message, args);
    }

    public static void LogCritical(EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(LogLevel.Critical, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogCritical(SocketTextChannel loggingChannel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.Critical, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogCritical(SocketGuild guild, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(guild, LogLevel.Critical, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogCritical(ulong id, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(id, LogLevel.Critical, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogCritical<TState>(EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(LogLevel.Critical, eventId, state, exception, formatter);
    }

    public static void LogCritical<TState>(SocketGuild guild, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(guild, LogLevel.Critical, eventId, state, exception, formatter);
    }

    public static void LogCritical<TState>(SocketTextChannel loggingChannel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Log(loggingChannel, LogLevel.Critical, eventId, state, exception, formatter);
    }

    public static void LogNone(string? message, params object?[] args)
    {
        Log(LogLevel.None, 0, null, message, args);
    }

    public static void LogNone(SocketTextChannel loggingChannel, string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.None, 0, null, message, args);
    }

    public static void LogNone(SocketGuild guild, string? message, params object?[] args)
    {
        Log(guild, LogLevel.None, 0, null, message, args);
    }

    public static void LogNone(ulong id, string? message, params object?[] args)
    {
        Log(id, LogLevel.None, 0, null, message, args);
    }

    public static void LogNone(EventId eventId, string? message, params object?[] args)
    {
        Log(LogLevel.None, eventId, null, message, args);
    }

    public static void LogNone(SocketTextChannel loggingChannel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(loggingChannel, LogLevel.None, eventId, null, message, args);
    }

    public static void LogNone(SocketGuild guild, EventId eventId, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.None, eventId, null, message, args);
    }

    public static void LogNone(ulong id, EventId eventId, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.None, eventId, null, message, args);
    }

    public static void LogNone(Exception exception, string? message, params object?[] args)
    {
        Log(LogLevel.None, 0, exception, message, args);
    }

    public static void LogNone(SocketTextChannel loggingChannel, Exception exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.None, 0, exception, message, args);
    }

    public static void LogNone(SocketGuild guild, Exception exception, string? message,
        params object?[] args)
    {
        Log(guild, LogLevel.None, 0, exception, message, args);
    }

    public static void LogNone(ulong id, Exception exception, string? message,
        params object?[] args)
    {
        Log(id, LogLevel.None, 0, exception, message, args);
    }

    public static void LogNone(EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(LogLevel.None, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogNone(SocketTextChannel loggingChannel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, LogLevel.None, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogNone(SocketGuild guild, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(guild, LogLevel.None, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogNone(ulong id, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(id, LogLevel.None, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void LogNone<TState>(EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(LogLevel.None, eventId, state, exception, formatter);
    }

    public static void LogNone<TState>(SocketGuild guild, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(guild, LogLevel.None, eventId, state, exception, formatter);
    }

    public static void LogNone<TState>(SocketTextChannel loggingChannel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Log(loggingChannel, LogLevel.None, eventId, state, exception, formatter);
    }

    public static void LogNone<TState>(ulong id, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Log(id, LogLevel.None, eventId, state, exception, formatter);
    }

    public static void Log(LogLevel logLevel, string? message, params object?[] args)
    {
        Log(logLevel, 0, null, message, args);
    }

    public static void Log(SocketTextChannel loggingChannel, LogLevel logLevel, string? message, params object?[] args)
    {
        Log(loggingChannel, logLevel, 0, null, message, args);
    }

    public static void Log(SocketGuild guild, LogLevel logLevel, string? message, params object?[] args)
    {
        Log(guild, logLevel, 0, null, message, args);
    }

    public static void Log(ulong id, LogLevel logLevel, string? message, params object?[] args)
    {
        Log(id, logLevel, 0, null, message, args);
    }

    public static void Log(LogLevel logLevel, EventId eventId, string? message, params object?[] args)
    {
        Log(logLevel, eventId, null, message, args);
    }

    public static void Log(SocketTextChannel loggingChannel, LogLevel logLevel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(loggingChannel, logLevel, eventId, null, message, args);
    }

    public static void Log(SocketGuild guild, LogLevel logLevel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(guild, logLevel, eventId, null, message, args);
    }

    public static void Log(ulong id, LogLevel logLevel, EventId eventId, string? message,
        params object?[] args)
    {
        Log(id, logLevel, eventId, null, message, args);
    }

    public static void Log(LogLevel logLevel, Exception exception, string? message, params object?[] args)
    {
        Log(logLevel, 0, exception, message, args);
    }

    public static void Log(SocketTextChannel loggingChannel, LogLevel logLevel, Exception exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, logLevel, 0, exception, message, args);
    }

    public static void Log(SocketGuild guild, LogLevel logLevel, Exception exception, string? message,
        params object?[] args)
    {
        Log(guild, logLevel, 0, exception, message, args);
    }

    public static void Log(ulong id, LogLevel logLevel, Exception exception, string? message,
        params object?[] args)
    {
        Log(id, logLevel, 0, exception, message, args);
    }

    public static void Log(LogLevel logLevel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(logLevel, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void Log(SocketTextChannel loggingChannel, LogLevel logLevel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(loggingChannel, logLevel, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void Log(SocketGuild guild, LogLevel logLevel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(guild, logLevel, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void Log(ulong id, LogLevel logLevel, EventId eventId, Exception? exception,
        string? message, params object?[] args)
    {
        Log(id, logLevel, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (Logger is null)
        {
            Console.WriteLine("CRITICAL: LOGGER IS NULL");
            return;
        }

        Logger._logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public static void Log<TState>(SocketGuild guild, LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (Logger is null)
        {
            Console.WriteLine("CRITICAL: LOGGER IS NULL");
            return;
        }

        if (!Logger._loggers.TryGetValue(guild, out SocketTextChannel? loggingChannel))
        {
            Logger._logger.LogError("The logging was prevented in the server {GuildName} ({GuildId}) " +
                                    "because no channel was registered as a logging channel for that server",
                guild.Name, guild.Id);
            return;
        }

        Log(loggingChannel, logLevel, eventId, state, exception, formatter);
    }

    public static void Log<TState>(SocketTextChannel loggingChannel, LogLevel logLevel, EventId eventId, TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        const int maximumLength = DiscordConfig.MaxMessageSize;

        if (Logger is null)
        {
            Console.WriteLine("CRITICAL: LOGGER IS NULL");
            return;
        }

        Logger._logger.Log(logLevel, eventId, state, exception, formatter);
        string message = formatter(state, exception);

        try
        {
            Task<RestUserMessage> messageSendTask = loggingChannel.SendMessageAsync(embed: new EmbedBuilder()
                .WithDescription(message).WithColor(logLevel switch
                {
                    LogLevel.Trace => Color.DarkerGrey,
                    LogLevel.Debug => Color.Gold,
                    LogLevel.Information => Color.LighterGrey,
                    LogLevel.Warning => Color.DarkOrange,
                    LogLevel.Error => Color.Red,
                    LogLevel.Critical => Color.Magenta,
                    LogLevel.None => Color.Default,
                    _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
                }).Build());

            messageSendTask.Wait();
        }
        catch (ArgumentOutOfRangeException argumentOutOfRangeException)
        {
            if (message.Length > maximumLength)
            {
                Logger._logger.LogError(argumentOutOfRangeException,
                    "Failed to send message to logging channel because it was too long." +
                    "The maximum is {MaximumLength} characters but this message was {MessageLength} characters long.\n" +
                    "That is {Difference} characters longer.", maximumLength, message.Length,
                    message.Length - maximumLength);
            }
            else
            {
                Logger._logger.LogError(argumentOutOfRangeException,
                    "Failed to send message to logging channel\n" +
                    "because the log level used has an int value not mapping " +
                    "to an actual enum value");
            }
        }
        catch (Exception exceptionFired)
        {
            Logger._logger.LogError(exceptionFired, "Failed to send message to logging channel");
        }
    }

    public static void Log<TState>(ulong id, LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (Logger is null)
        {
            Console.WriteLine("CRITICAL: LOGGER IS NULL");
            return;
        }

        if (!Logger._loggerIds.TryGetValue(id, out SocketTextChannel? loggingChannel))
        {
            Logger._logger.LogError("The logging was prevented for the channel with ID {ChannelId} " +
                                    "because no channel was registered with that ID", id);
            return;
        }

        Log(loggingChannel, logLevel, eventId, state, exception, formatter);
    }

    public static bool IsEnabled(LogLevel logLevel)
    {
        return Logger is not null && Logger._logger.IsEnabled(logLevel);
    }

    public static IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return Logger?._logger.BeginScope(state);
    }

    public static bool RegisterLogger(SocketGuild guild, SocketTextChannel channel)
    {
        if (Logger is null)
        {
            return false;
        }

        if (Logger._loggers.TryAdd(guild, channel))
        {
            return true;
        }

        Logger._loggers[guild] = channel;
        return true;
    }

    public static SocketTextChannel? GetLogger(SocketGuild guild)
    {
        if (Logger is null)
        {
            return null;
        }

        if (Logger._loggers.TryGetValue(guild, out SocketTextChannel? logger))
        {
            return logger;
        }

        return null;
    }

    private static string MessageFormatter(FormattedLogValues state, Exception? error)
    {
        return state.ToString();
    }
}
