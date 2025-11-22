using ArkaZilla.Technology;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ArkaZilla.Services;

public class ChannelService
{
    public static ChannelService Instance { get; } = new();

    private HashSet<SocketGuild> _servers { get; } = [];

    private Dictionary<ulong, SocketGuild> _serversById { get; } = [];

    private Dictionary<string, HashSet<SocketGuild>> _serversByName { get; } = [];

    private HashSet<SocketCategoryChannel> _categoryChannels { get; } = [];

    private Dictionary<ulong, SocketCategoryChannel> _categoryChannelsById { get; } = [];

    private Dictionary<string, HashSet<SocketCategoryChannel>> _categoryChannelsByName { get; } = [];

    private HashSet<SocketGuildChannel> _textChannels { get; } = [];

    private Dictionary<ulong, SocketGuildChannel> _textChannelsById { get; } = [];

    private Dictionary<string, HashSet<SocketGuildChannel>> _textChannelsByName { get; } = [];

    private HashSet<SocketVoiceChannel> _voiceChannels { get; } = [];

    private Dictionary<ulong, SocketVoiceChannel> _voiceChannelsById { get; } = [];

    private Dictionary<string, HashSet<SocketVoiceChannel>> _voiceChannelsByName { get; } = [];

    private HashSet<SocketGuildChannel> _serverChannels { get; } = [];

    private Dictionary<ulong, SocketGuildChannel> _serverChannelsById { get; } = [];

    private Dictionary<string, HashSet<SocketGuildChannel>> _serverChannelsByName { get; } = [];

    private HashSet<SocketGroupChannel> _groupChannels { get; init; } = [];

    private Dictionary<ulong, SocketGroupChannel> _groupChannelsById { get; init; } = [];

    private Dictionary<ulong, SocketGroupChannel> _groupChannelsByUserId { get; init; } = [];

    private Dictionary<SocketUser, HashSet<SocketGroupChannel>> _groupChannelsByUser { get; init; } = [];

    private Dictionary<string, HashSet<SocketGroupChannel>> _groupChannelsByUserName { get; init; } = [];

    private HashSet<SocketDMChannel> _dmChannels { get; init; } = [];

    private Dictionary<ulong, SocketDMChannel> _dmChannelsById { get; init; } = [];

    private Dictionary<ulong, SocketDMChannel> _dmChannelsByUserId { get; init; } = [];

    private Dictionary<SocketUser, SocketDMChannel> _dmChannelsByUser { get; init; } = [];

    private Dictionary<string, SocketDMChannel> _dmChannelsByUserName { get; init; } = [];

    private ChannelService()
    { }

    public void Configure(DiscordSocketClient client)
    {
        client.JoinedGuild += guild =>
        {
            AddGuild(guild);
            return Task.CompletedTask;
        };

        client.GuildAvailable += guild =>
        {
            AddGuild(guild);
            return Task.CompletedTask;
        };

        client.GuildUnavailable += guild =>
        {
            RemoveGuild(guild);
            return Task.CompletedTask;
        };

        client.LeftGuild += guild =>
        {
            RemoveGuild(guild);
            return Task.CompletedTask;
        };

        client.GuildUpdated += (before, after) =>
        {
            UpdateGuild(before, after);
            return Task.CompletedTask;
        };

        client.ChannelCreated += channel =>
        {
            AddChannel(channel);
            return Task.CompletedTask;
        };

        client.ChannelDestroyed += channel =>
        {
            RemoveChannel(channel);
            return Task.CompletedTask;
        };

        client.ChannelUpdated += (before, after) =>
        {
            UpdateChannel(before, after);
            return Task.CompletedTask;
        };
    }


    private void AddGuild(SocketGuild guild)
    {
        _servers.Add(guild);
        _serversById.Add(guild.Id, guild);

        if (_serversByName.TryGetValue(guild.Name, out var guilds))
        {
            guilds.Add(guild);
        }
        else
        {
            _serversByName.Add(guild.Name, [guild]);
        }

        foreach (var categoryChannel in guild.CategoryChannels)
        {
            AddCategory(categoryChannel);
        }
    }

    private void AddCategory(SocketCategoryChannel categoryChannel)
    {
        _categoryChannels.Add(categoryChannel);
        _categoryChannelsById.Add(categoryChannel.Id, categoryChannel);

        if (_categoryChannelsByName.TryGetValue(categoryChannel.Name, out var channels))
        {
            channels.Add(categoryChannel);
        }
        else
        {
            _categoryChannelsByName.Add(categoryChannel.Name, [categoryChannel]);
        }

        foreach (SocketGuildChannel channel in categoryChannel.Channels)
        {
            AddChannel(channel);
        }
    }

    private void AddChannel(SocketChannel channel)
    {
        switch (channel)
        {
            case SocketGuildChannel guildChannel:
                AddChannel(guildChannel);
                break;
            case SocketGroupChannel groupChannel:
                AddChannel(groupChannel);
                break;
            case SocketDMChannel dmChannel:
                AddChannel(dmChannel);
                break;
        }
    }

    private void AddChannel(SocketGuildChannel guildChannel)
    {
        if (guildChannel is SocketTextChannel)
        {
            _textChannels.Add(guildChannel);
            _textChannelsById.Add(guildChannel.Id, guildChannel);

            if (_textChannelsByName.TryGetValue(guildChannel.Name, out var textChannels))
            {
                textChannels.Add(guildChannel);
            }
            else
            {
                _textChannelsByName.Add(guildChannel.Name, [guildChannel]);
            }
        }
        else if (guildChannel is SocketVoiceChannel voiceChannel)
        {
            _voiceChannels.Add(voiceChannel);
            _voiceChannelsById.Add(voiceChannel.Id, voiceChannel);

            if (_voiceChannelsByName.TryGetValue(voiceChannel.Name, out var voiceChannels))
            {
                voiceChannels.Add(voiceChannel);
            }
            else
            {
                _voiceChannelsByName.Add(voiceChannel.Name, [voiceChannel]);
            }
        }

        _serverChannels.Add(guildChannel);
        _serverChannelsById.Add(guildChannel.Id, guildChannel);

        if (_serverChannelsByName.TryGetValue(guildChannel.Name, out var serverChannels))
        {
            serverChannels.Add(guildChannel);
        }
        else
        {
            _serverChannelsByName.Add(guildChannel.Name, [guildChannel]);
        }
    }

    private void AddChannel(SocketGroupChannel groupChannel)
    {
        _groupChannels.Add(groupChannel);
        _groupChannelsById.Add(groupChannel.Id, groupChannel);

        foreach (var user in groupChannel.Users)
        {
            if (_groupChannelsByUser.TryGetValue(user, out var channels))
            {
                channels.Add(groupChannel);
            }
            else
            {
                _groupChannelsByUser.Add(user, [groupChannel]);
            }

            if (_groupChannelsByUserName.TryGetValue(user.Username, out var channelsByName))
            {
                channelsByName.Add(groupChannel);
            }
            else
            {
                _groupChannelsByUserName.Add(user.Username, [groupChannel]);
            }
        }
    }

    private void AddChannel(SocketDMChannel dmChannel)
    {
        _dmChannels.Add(dmChannel);
        _dmChannelsById.Add(dmChannel.Id, dmChannel);

        _dmChannelsByUserId.Add(dmChannel.Recipient.Id, dmChannel);
        _dmChannelsByUser.Add(dmChannel.Recipient, dmChannel);
        _dmChannelsByUserName.Add(dmChannel.Recipient.Username, dmChannel);
    }

    private void UpdateGuild(SocketGuild before, SocketGuild after)
    {
        if (before.Name == after.Name)
        {
            return;
        }

        bool gotServers = _serversByName.TryGetValue(before.Name, out HashSet<SocketGuild>? guilds);
        if (!gotServers)
        {
            // TODO: log this
            return;
        }
        else if (guilds is null)
        {
            // TODO: log this
            return;
        }

        guilds.Remove(before);
        if (!_serversByName.TryGetValue(after.Name, out HashSet<SocketGuild>? guildsAfter))
        {
            if (guildsAfter is null)
            {
                // TODO: log this
                return;
            }

            _serversByName.Add(after.Name, [after]);
            return;
        }

        guildsAfter.Add(after);
    }

    private void UpdateChannel(SocketChannel before, SocketChannel after)
    {
        throw new NotImplementedException();
    }

    private void RemoveGuild(SocketGuild guild)
    {
        foreach (var categoryChannel in guild.CategoryChannels)
        {
            RemoveCategory(categoryChannel);
        }

        _servers.Remove(guild);
        _serversById.Remove(guild.Id);

        HashSet<SocketGuild> guilds = _serversByName[guild.Name];
        guilds.Remove(guild);
        if (guilds.Count == 0)
        {
            _serversByName.Remove(guild.Name);
        }
    }

    private void RemoveCategory(SocketCategoryChannel categoryChannel)
    {
        foreach (var channel in categoryChannel.Channels)
        {
            RemoveChannel(channel);
        }

        _categoryChannels.Remove(categoryChannel);
        _categoryChannelsById.Remove(categoryChannel.Id);

        HashSet<SocketCategoryChannel> channels = _categoryChannelsByName[categoryChannel.Name];
        channels.Remove(categoryChannel);
        if (channels.Count == 0)
        {
            _categoryChannelsByName.Remove(categoryChannel.Name);
        }
    }

    private void RemoveChannel(SocketChannel channel)
    {
        throw new NotImplementedException();
    }

    private void RemoveChannel(SocketGuildChannel channel)
    {
        _serverChannels.Remove(channel);
        _serverChannelsById.Remove(channel.Id);

        HashSet<SocketGuildChannel> serverChannels = _serverChannelsByName[channel.Name];
        serverChannels.Remove(channel);
        if (serverChannels.Count == 0)
        {
            _serverChannelsByName.Remove(channel.Name);
        }

        if (channel is SocketTextChannel textChannel)
        {
            _textChannels.Remove(textChannel);
            _textChannelsById.Remove(textChannel.Id);

            HashSet<SocketGuildChannel> textChannels = _textChannelsByName[textChannel.Name];
            textChannels.Remove(textChannel);
            if (textChannels.Count == 0)
            {
                _textChannelsByName.Remove(textChannel.Name);
            }
        }
        else if (channel is SocketVoiceChannel voiceChannel)
        {
            _voiceChannels.Remove(voiceChannel);
            _voiceChannelsById.Remove(voiceChannel.Id);

            HashSet<SocketVoiceChannel> voiceChannels = _voiceChannelsByName[voiceChannel.Name];
            voiceChannels.Remove(voiceChannel);
            if (voiceChannels.Count == 0)
            {
                _voiceChannelsByName.Remove(voiceChannel.Name);
            }
        }
    }

    public class GuildAutocompleteHandler : AutocompleteHandler
    {
        public static Dictionary<string, Func<SocketGuild, bool, string>> ResultFactories { get; } = new();

        private static readonly Lock Lock = new();

        public interface IGuildResultFactory
        {
            string GetResult(SocketGuild guild, bool isId);
        }

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            string factoryKey = $"{parameter.Command.Name}|{parameter.Name}";

            if (!ResultFactories.ContainsKey(factoryKey))
            {
                lock (Lock)
                {
                    if (!ResultFactories.ContainsKey(factoryKey))
                    {
                        ResultFactoryAttribute? attribute = parameter.Attributes.OfType<ResultFactoryAttribute>().FirstOrDefault();
                        if (attribute is not null && typeof(IGuildResultFactory).IsAssignableFrom(attribute.FactoryType))
                        {
                            IGuildResultFactory instance = (IGuildResultFactory)Activator.CreateInstance(attribute.FactoryType)!;
                            ResultFactories[factoryKey] = instance.GetResult;
                        }
                    }
                }
            }

            string currentValue = autocompleteInteraction.Data.Current.Value?.ToString() ?? string.Empty;

            List<AutocompleteResult> scoredRoles = Instance._servers
                .Select(guild =>
                {

                    int fuzzyScoreGuild = Searching.CalculateFuzzyScore(guild.Name, currentValue);
                    int fuzzyScoreId = Searching.CalculateFuzzyScore(guild.Id.ToString(), currentValue);
                    bool isId = fuzzyScoreId > fuzzyScoreGuild;
                    int fuzzyScore = isId ? fuzzyScoreId : fuzzyScoreGuild;

                    return new { Guild = guild, IsId = isId, Score = fuzzyScore };
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(Searching.DiscordSuggestionsLimit)
                .Select((string display, string value) (result) =>
                {
                    if (ResultFactories.TryGetValue(factoryKey, out Func<SocketGuild, bool, string>? factory))
                    {
                        return (factory(result.Guild, result.IsId), result.Guild.Id.ToString());
                    }

                    return (GetResultDefault(result.Guild, result.IsId), result.Guild.Id.ToString());
                })
                .Select(result => new AutocompleteResult(result.display, result.value))
                .ToList();

            return Task.FromResult(AutocompletionResult.FromSuccess(scoredRoles));
        }

        private static string GetResultDefault(SocketGuild guild, bool isId)
        {
            if (!isId)
            {
                return $"{guild.Id} ({guild.Name})";
            }

            return $"{guild.Name} ({guild.Id})";
        }

        [AttributeUsage(AttributeTargets.Parameter)]
        public class ResultFactoryAttribute : Attribute
        {
            public Type FactoryType { get; }

            public ResultFactoryAttribute(Type factoryType)
            {
                if (!typeof(IGuildResultFactory).IsAssignableFrom(factoryType))
                {
                    throw new ArgumentException($"Type {factoryType.FullName} does not implement {nameof(IGuildResultFactory)}");
                }

                FactoryType = factoryType;
            }
        }
    }
}
