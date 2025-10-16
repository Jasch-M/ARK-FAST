
/*
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace ArkaZilla.Services;

public class RoleAssociationService : IServiceProvider
{
    private Dictionary<SocketRole, HashSet<SocketRole>> _roleAssociations { get; init; }

    private const string RelativeFilePath = "../../../Assets/channel_pairs.json";

    private DiscordSocketClient _client;

    private readonly IDiscordBotUserService _botUserService;

    private readonly HashSet<SocketRole> _allRoles;

    private readonly Dictionary<string, HashSet<SocketRole>> _allRolesByName;

    private readonly Dictionary<string, List<HashSet<SocketRole>>> _roleAssociationsByName;

    private readonly Dictionary<ulong, SocketRole> _roleLookup;

    public RoleAssociationService(IDiscordBotUserService botUserService)
    {
        _botUserService = botUserService;
        _client = botUserService.Client;
        (_roleAssociations, _roleAssociationsByName) = LoadRoleAssociations(botUserService);

        _allRoles = new HashSet<SocketRole>();
        _allRolesByName = new Dictionary<string, HashSet<SocketRole>>();
        _roleLookup = new Dictionary<ulong, SocketRole>();

        FillAllRoles();
    }

    private void FillAllRoles()
    {
        foreach (SocketGuild guild in _client.Guilds)
        {
            IReadOnlyCollection<SocketRole>? roles = guild.Roles;
            foreach (SocketRole role in roles)
            {
                _allRoles.Add(role);
                _roleLookup.Add(role.Id, role);

                if (!_allRolesByName.TryGetValue(role.Name, out HashSet<SocketRole>? roleSet)) // TODO: fix this logic
                {
                    roleSet = [role];
                    _allRolesByName.Add(role.Name, roleSet);
                    _allRolesByName.Add(role.Id.ToString(), roleSet);
                    continue;
                }

                roleSet.Add(role);
                _allRolesByName.Add(role.Id.ToString(), roleSet);
            }
        }
    }

    private static (Dictionary<SocketRole, HashSet<SocketRole>> roleAssociations,
        Dictionary<string, List<HashSet<SocketRole>>> roleAssociationsByName) LoadRoleAssociations(IDiscordBotUserService client)
    {
        if (!File.Exists(RelativeFilePath))
        {
            return (new Dictionary<SocketRole, HashSet<SocketRole>>(), new Dictionary<string, List<HashSet<SocketRole>>>());
        }

        string json = File.ReadAllText(RelativeFilePath);
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json);
        }
        catch (JsonException jsonException)
        {
            client.Logger.LogError(
                "Failed to parse role associations JSON as the Json was invalid.\n{JsonExceptionMessage}" +
                "\nLoading empty associations!, <@557680932463706213>", jsonException);
            return (new Dictionary<SocketRole, HashSet<SocketRole>>(), new Dictionary<string, List<HashSet<SocketRole>>>());
        }
        catch (ArgumentException argumentException)
        {
            client.Logger.LogError("Failed to parse role associations as the json text was null. " +
                                   "This is an unreachable exception and should never be triggered unless there is a bug. " +
                                   "<@557680932463706213>.\n{ArgumentException}", argumentException);
            return (new Dictionary<SocketRole, HashSet<SocketRole>>(), new Dictionary<string, List<HashSet<SocketRole>>>());
        }

        if (root is null)
        {
            client.Logger.LogError("Failed to parse role associations as the json root node was null. " +
                                   "This means the json file was empty or contained only whitespace. " +
                                   "Loading empty associations! <@557680932463706213>");
            return (new Dictionary<SocketRole, HashSet<SocketRole>>(), new Dictionary<string, List<HashSet<SocketRole>>>());
        }

        JsonArray array;

        try
        {
            array = root.AsArray();
        }
        catch (InvalidOperationException invalidOperationException)
        {
            client.Logger.LogError(
                "The root node is not an array but is a {JsonRootNodeType} for role associations in the JSON file.\n" +
                "Loading an empty set of associations! <@557680932463706213>\nThe exception is {Exception}",
                root.GetType(), invalidOperationException);
            return (new Dictionary<SocketRole, HashSet<SocketRole>>(), new Dictionary<string, List<HashSet<SocketRole>>>());
        }

        Dictionary<SocketRole, HashSet<SocketRole>> roleAssociations = new();
        Dictionary<string, List<HashSet<SocketRole>>> roleAssociationsByName = new();
        for (int index = 0; index < array.Count; index++)
        {
            JsonNode? jsonNode = array[(Index)index];
            if (jsonNode is null)
            {
                client.Logger.LogWarning(
                    "the {indexString} json node in the root array of the role JSON association file was null. This node will be skipped.",
                    index.ToPrettyString());
                continue;
            }

            JsonArray roleArray;
            try
            {
                roleArray = jsonNode.AsArray();
            }
            catch (InvalidOperationException invalidOperationException)
            {
                client.Logger.LogError(
                    "The {indexString} element in the root array is not an array but is a {JsonNodeType} in the role associations JSON file.\n" +
                    "This node will be skipped! <@557680932463706213>\nThe exception is {Exception}",
                    index.ToPrettyString(), jsonNode?.GetType(), invalidOperationException);

                continue;
            }

            HashSet<SocketRole> associatedRoles = [];

            for (int roleIndex = 0; roleIndex < roleArray.Count; roleIndex++)
            {
                JsonNode? roleIdNode = roleArray[roleIndex];
                if (roleIdNode is null)
                {
                    client.Logger.LogWarning(
                        "The {roleIndexString} role id node in the {indexString} array of the role JSON association file was null. This role will be skipped.",
                        roleIndex.ToPrettyString(), index.ToPrettyString());
                    continue;
                }

                if (roleIdNode.GetType() != typeof(JsonValue))
                {
                    client.Logger.LogError(
                        "The {roleIndexString} role id node in the {indexString} array is not a string but is a {JsonNodeType} in the role associations JSON file.\n" +
                        "This role will be skipped! <@557680932463706213>",
                        roleIndex.ToPrettyString(), index.ToPrettyString(), roleIdNode.GetType());
                    continue;
                }

                string value = roleIdNode.GetValue<string>();
                if (!ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong roleId))
                {
                    client.Logger.LogError(
                        "Failed to parse role id '{RoleId}' in the {roleIndexString} role id node in the {indexString} array of the role JSON association file.\n" +
                        "This role will be skipped! <@557680932463706213>",
                        value, roleIndex.ToPrettyString(), index.ToPrettyString());
                    continue;
                }

                IReadOnlyCollection<SocketGuild> guilds = client.Client.Guilds;
                List<SocketRole> candidateRoles = [];
                foreach (SocketGuild guild in guilds)
                {
                    if (guild.GetRole(roleId) is not SocketRole role)
                    {
                        continue;
                    }

                    candidateRoles.Add(role);
                }

                if (candidateRoles.Count == 0)
                {
                    client.Logger.LogWarning(
                        "Failed to find role with id '{RoleId}' in any guild in the role JSON association file.\n" +
                        "This role will be skipped! <@557680932463706213>",
                        roleId);
                    continue;
                }

                if (candidateRoles.Count > 1)
                {
                    client.Logger.LogWarning(
                        "Multiple roles with id '{RoleId}' found across different guilds. All of these roles will be added as a single association.",
                        roleId);
                }

                foreach (SocketRole role in candidateRoles)
                {
                    associatedRoles.Add(role);
                    roleAssociations.Add(role, associatedRoles);

                    if (!roleAssociationsByName.TryGetValue(role.Name, out List<HashSet<SocketRole>>? roleAssociationsList))
                    {
                        roleAssociationsByName[role.Name] = [associatedRoles];
                    }
                    else
                    {
                        HashSet<SocketRole>? associatedRolesNameGroup = null;
                        foreach (HashSet<SocketRole> roleGroupByName in roleAssociationsList.Where(t => ReferenceEquals(associatedRoles, t)))
                        {
                            associatedRolesNameGroup = roleGroupByName;
                            break;
                        }

                        if (associatedRolesNameGroup is null)
                        {
                            roleAssociationsList.Add(associatedRoles);
                        }
                    }
                }
            }
        }

        return (roleAssociations, roleAssociationsByName);
    }

    public bool SaveRoleAssociations(IDiscordBotUserService client)
    {
        string json = GenerateJsonRepresentation();

        try
        {
            using StreamWriter writer = File.CreateText(RelativeFilePath);
            writer.Write(json);
            writer.Close();

            client.Logger.LogInformation("Saved role associations to {FilePath}", RelativeFilePath);
            return true;
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            client.Logger.LogError(
                "Failed to save role associations as the file was not writable. <@557680932463706213>.\n" +
                "The program does not have permissions to access or write to the file.\n{UnauthorizedAccessException}",
                unauthorizedAccessException);
            return false;
        }
        catch (ArgumentNullException argumentNullExceptionException)
        {
            client.Logger.LogError(
                "Failed to save role association as the file path string was null. <@557680932463706213>.\n" +
                "This is an active comment and should be an unreachable exception. If this was triggered, " +
                "there is a bug in the code.\n{ArgumentNullException}",
                argumentNullExceptionException);
            return false;
        }
        catch (ArgumentException argumentException)
        {
            client.Logger.LogError(
                "Failed to save role association as the file path string was empty. <@557680932463706213>.\n" +
                "This is an active comment and should be an unreachable exception. If this was triggered, " +
                "there is a bug in the code.\n{ArgumentException}",
                argumentException);
            return false;
        }
        catch (PathTooLongException pathTooLongException)
        {
            client.Logger.LogError(
                "Failed to save role association as the file path string was too long. <@557680932463706213>.\n" +
                "This is an active comment and should be an unreachable exception. If this was triggered, " +
                "there is a bug in the code.\n{PathTooLongException}",
                pathTooLongException);
            return false;
        }
        catch (DirectoryNotFoundException directoryNotFoundException)
        {
            client.Logger.LogError(
                "Failed to save role association as the directory containing the file was not found. <@557680932463706213>." +
                "\n{DirectoryNotFoundException}", directoryNotFoundException);
            return false;
        }
        catch (NotSupportedException notSupportedException)
        {
            client.Logger.LogError(
                "Failed to save role association as opening the file seems to be an unsupported exception.\n" +
                "This is an active comment and should be an unreachable exception. If this was triggered, " +
                "there is a bug in the code.\n{NotSupportedException}", notSupportedException);
            return false;
        }
    }

    [return: LanguageInjection(InjectedLanguage.JSON)]
    private string GenerateJsonRepresentation()
    {
        const int indentationWidth = 4;

        StringBuilder representationBuilder = new();
        representationBuilder.Append("[\n");
        int indententation = indentationWidth;

        HashSet<SocketRole> roles = new();
        List<KeyValuePair<SocketRole, HashSet<SocketRole>>> roleAssociations = _roleAssociations.ToList();
        for (int index = 0; index < roleAssociations.Count; index++)
        {
            KeyValuePair<SocketRole, HashSet<SocketRole>> roleAssociation = roleAssociations[index];
            if (roles.Contains(roleAssociation.Key))
            {
                continue;
            }

            HashSet<SocketRole> associatedRoles = roleAssociation.Value;
            roles.UnionWith(associatedRoles);

            representationBuilder.Append(' ', indententation);
            representationBuilder.Append("[\n");
            indententation += indentationWidth;

            List<SocketRole> associatedRolesList = associatedRoles.ToList();
            for (int i = 0; i < associatedRolesList.Count; i++)
            {
                SocketRole associatedRole = associatedRolesList[i];
                representationBuilder.Append(' ', indententation);
                representationBuilder.Append($"\"{associatedRole.Id}\"");
                if (i < associatedRolesList.Count - 1)
                {
                    representationBuilder.Append(',');
                }

                representationBuilder.Append('\n');
            }

            indententation -= indentationWidth;
            representationBuilder.Append(' ', indententation);
            representationBuilder.Append(']');
            if (index < roleAssociations.Count - 1)
            {
                representationBuilder.Append(',');
            }

            representationBuilder.Append('\n');
        }

        representationBuilder.Append(']');
        return representationBuilder.ToString();
    }

    public bool AddRoleAssociation(SocketRole role, SocketRole associatedRole, bool save = true)
    {
        if (role == associatedRole)
        {
            _botUserService.Logger.LogWarning(
                "Attempted to add a role association for role {RoleId} to itself. This is not allowed.", role.Id);
            return false;
        }

        if (!_roleAssociations.TryGetValue(role, out HashSet<SocketRole>? associatedRolesRoleSet))
        {
            if (!_roleAssociations.TryGetValue(associatedRole,
                    out HashSet<SocketRole>? associatedRolesAssociatedRoleSet))
            {
                HashSet<SocketRole> associatedRoles =
                [
                    role,
                    associatedRole
                ];

                _roleAssociations.Add(associatedRole, associatedRoles);
                _roleAssociations.Add(role, associatedRoles);
            }
            else
            {
                associatedRolesAssociatedRoleSet.Add(role);
                _roleAssociations.Add(role, associatedRolesAssociatedRoleSet);
            }
        }
        else
        {
            if (!_roleAssociations.TryGetValue(associatedRole,
                    out HashSet<SocketRole>? associatedRolesAssociatedRoleSet))
            {
                associatedRolesRoleSet.Add(associatedRole);
                _roleAssociations.Add(associatedRole, associatedRolesRoleSet);
            }
            else
            {
                _botUserService.Logger.LogWarning(
                    "Be careful that the operation is merging two previously distinct role association groups.");

                associatedRolesRoleSet.UnionWith(associatedRolesAssociatedRoleSet);
                foreach (SocketRole associatedRoleInGroup in associatedRolesAssociatedRoleSet)
                {
                    _roleAssociations[associatedRoleInGroup] = associatedRolesRoleSet;
                }
            }
        }

        if (save)
        {
            return SaveRoleAssociations(_botUserService);
        }

        return true;
    }

    public bool AddRole(SocketRole role, bool save = true)
    {
        if (_roleAssociations.TryGetValue(role, out HashSet<SocketRole>? _))
        {
            _botUserService.Logger.LogWarning(
                "Attempted to add a role that already has a saved association. ({Role} in {Server})", role.Name,
                role.Guild.Name);
            return false;
        }

        HashSet<SocketRole> associatedRoles = [role];
        _roleAssociations.Add(role, associatedRoles);

        if (save)
        {
            return SaveRoleAssociations(_botUserService);
        }

        return true;
    }

    public bool RemoveRole(SocketRole role, bool save = true)
    {
        if (!_roleAssociations.TryGetValue(role, out HashSet<SocketRole>? associatedRoles))
        {
            _botUserService.Logger.LogWarning(
                "Attempted to remove a role that does not have a saved association. ({Role} in {Server})", role.Name,
                role.Guild.Name);
            return false;
        }

        associatedRoles.Remove(role);
        _roleAssociations.Remove(role);

        if (save)
        {
            return SaveRoleAssociations(_botUserService);
        }

        return true;
    }

    public bool RemoveRoleAssociation(SocketRole role, SocketRole associatedRole, bool save = true)
    {
        if (!_roleAssociations.TryGetValue(role, out HashSet<SocketRole>? associatedRolesRoleSet))
        {
            _botUserService.Logger.LogWarning(
                "Attempted to remove a role association that does not exist between the {Role} in {Server} role and the {OtherRole} in {OtherServer}" +
                " as the {Role2} role in the {Server2} does not exist in any saved association",
                role.Name, role.Guild.Name, associatedRole.Name, associatedRole.Guild.Name, role.Name, role.Guild.Name);
            return false;
        }

        if (!_roleAssociations.TryGetValue(role, out HashSet<SocketRole>? associatedRolesAssociatedRoleSet))
        {
            _botUserService.Logger.LogWarning(
                "Attempted to remove a role association that does not exist between the {Role} in {Server} role and the {OtherRole} in {OtherServer}" +
                " as the {Role2} role in the {Server2} does not exist in any saved association",
                role.Name, role.Guild.Name, associatedRole.Name, associatedRole.Guild.Name, associatedRole.Name,
                associatedRole.Guild.Name);
        }

        if (!ReferenceEquals(associatedRolesRoleSet, associatedRolesAssociatedRoleSet))
        {
            _botUserService.Logger.LogWarning(
                "Attempted to remove a role association that does not exist between the {Role} role in the {Server} and the other {OtherRole} role in the {OtherServer}.\n" +
                "The roles have saved associations but are not tied to each other",
                role.Name, role.Guild.Name, associatedRole.Name, associatedRole.Guild.Name);
        }

        HashSet<SocketRole> associatedRoles = [associatedRole];
        associatedRolesRoleSet.Remove(associatedRole);

        _roleAssociations[associatedRole] = associatedRoles;

        if (save)
        {
            return SaveRoleAssociations(_botUserService);
        }

        return true;
    }

    public List<IReadOnlySet<SocketRole>> GetRoleAssociations()
    {
        List<IReadOnlySet<SocketRole>> roleAssociations = [];
        roleAssociations.AddRange(_roleAssociations.Select(roleAssociation => roleAssociation.Value));

        return roleAssociations;
    }

    public SocketRole? LookupRole(ulong roleId)
    {
        return _roleLookup.TryGetValue(roleId, out SocketRole? value) ? value : null;
    }

    public void CreateRole(SocketRole role)
    {
        _allRoles.Add(role);

        if (_allRolesByName.TryGetValue(role.Name, out HashSet<SocketRole>? value))
        {
            value.Add(role);
        }
        else
        {
            HashSet<SocketRole> valueSet = [role];
            _allRolesByName.Add(role.Name, valueSet);
        }

        _roleLookup.Add(role.Id, role);
    }

    public void DeleteRole(SocketRole role)
    {
        _allRoles.Remove(role);
        _allRolesByName.Remove(role.Name);
        _roleLookup.Remove(role.Id);

        if (!_roleAssociations.TryGetValue(role, out HashSet<SocketRole>? associatedRoles))
        {
            return;
        }

        associatedRoles.Remove(role);

        _roleAssociations.Remove(role);

        if (associatedRoles.Count == 0 || associatedRoles.All(thisRoleName => thisRoleName.Name != role.Name))
        {
            List<HashSet<SocketRole>> roleAssociationsList = _roleAssociationsByName[role.Name];
            roleAssociationsList.Remove(associatedRoles);
        }
    }

    public class BaseRoleAutocompleteHandler : AutocompleteHandler
    {
        protected const int DiscordSuggestionsLimit = 25;

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            object? service = services.GetService(typeof(DiscordBotUserService));
            if (service is not DiscordBotUserService discordBotUserService)
            {
                return Task.FromResult(AutocompletionResult.FromError(
                    new ArgumentException("Failed to get the discord bot user service")));
            }

            string currentValue = autocompleteInteraction.Data.Current.Value as string ?? string.Empty;
            if (currentValue.Length < 2)
            {
                return Task.FromResult(AutocompletionResult.FromSuccess(new List<AutocompleteResult>()));
            }

            List<AutocompleteResult> scoredRoles = discordBotUserService.RoleAssociationService._allRoles
                .Select(role =>
                {
                    int fuzzyScoreRole = CalculateFuzzyScore(role.Name, currentValue);
                    int fuzzyScoreId = CalculateFuzzyScore(role.Id.ToString(), currentValue);
                    bool isId = fuzzyScoreId > fuzzyScoreRole;
                    int fuzzyScore = isId ? fuzzyScoreId : fuzzyScoreRole;

                    return new
                    {
                        Role = role,
                        IsId = isId,
                        Score = fuzzyScore
                    };
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(DiscordSuggestionsLimit)
                .Select(x =>
                {
                    if (!x.IsId)
                    {
                        return new AutocompleteResult($"{x.Role.Name} ({x.Role.Guild.Name})", x.Role.Id.ToString());
                    }

                    return new AutocompleteResult($"{x.Role.Id} ({x.Role.Name} ({x.Role.Guild.Name}))", x.Role.Id.ToString());
                })
                .ToList();

            return Task.FromResult(AutocompletionResult.FromSuccess(scoredRoles));
        }

        internal static int CalculateFuzzyScore(string source, string query)
        {
            if (string.IsNullOrEmpty(query)) return 1;

            // Handle numeric ID matching
            if (ulong.TryParse(source, out ulong sourceId) && ulong.TryParse(query, out ulong queryId))
            {
                string sourceStr = sourceId.ToString();
                string queryStr = queryId.ToString();
                if (sourceStr.StartsWith(queryStr))
                {
                    return 1000 - (sourceStr.Length - queryStr.Length);
                }
                return 0;
            }

            source = source.ToLowerInvariant();
            query = query.ToLowerInvariant();

            if (source == query) return 1000;

            if (source.StartsWith(query))
            {
                return 800 - source.Length;
            }

            int score = 0;
            int queryIndex = 0;
            bool firstMatchFound = false;
            int consecutiveMatches = 0;

            for (int sourceIndex = 0; sourceIndex < source.Length; sourceIndex++)
            {
                if (queryIndex < query.Length && source[sourceIndex] == query[queryIndex])
                {
                    if (!firstMatchFound)
                    {
                        score += 200 - (sourceIndex * 10);
                        firstMatchFound = true;
                    }

                    consecutiveMatches++;
                    score += 50 * consecutiveMatches;

                    queryIndex++;
                }
                else
                {
                    consecutiveMatches = 0;
                }
            }

            if (queryIndex != query.Length)
            {
                return 0;
            }

            return Math.Max(1, score - source.Length);
        }
    }

    public class AssociatedRoleAutocompleteHandler : BaseRoleAutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            object? service = services.GetService(typeof(DiscordBotUserService));
            if (service is not DiscordBotUserService discordBotUserAssociationService)
            {
                return Task.FromResult(AutocompletionResult.FromError(
                    new ArgumentException("Failed to get the discord bot user service")));
            }

            string currentValue = autocompleteInteraction.Data.Current.Value as string ?? string.Empty;
            if (currentValue.Length < 2)
            {
                return Task.FromResult(AutocompletionResult.FromSuccess(new List<AutocompleteResult>()));
            }

            List<AutocompleteResult> scoredRoles = discordBotUserAssociationService.RoleAssociationService._roleAssociationsByName.Keys
                .Select(role => new { Role = role, Score = CalculateFuzzyScore(role, currentValue) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(DiscordSuggestionsLimit)
                .Select(x => new AutocompleteResult($"{x.Role}", x.Role))
                .ToList();

            return Task.FromResult(AutocompletionResult.FromSuccess(scoredRoles));
        }
    }

    public class BaseRolePrecondition : ParameterPreconditionAttribute
    {
        protected Task<PreconditionResult> CheckRequirementsAsyncCore(object value,
            IServiceProvider services, Func<RoleAssociationService, string, bool> check)
        {
            if (value is not string && value is not List<string>)
            {
                return Task.FromResult(PreconditionResult.FromError("The value must be either a string or a list of strings"));
            }

            object? service = services.GetService(typeof(DiscordBotUserService));
            if (service is not DiscordBotUserService discordBotUserService)
            {
                return Task.FromResult(PreconditionResult.FromError("The service is not a DiscordBotUserService. " +
                                                                    "This is a bug. Please report this to my maker."));
            }

            if (value is string singleValue)
            {
                if (check(discordBotUserService.RoleAssociationService, singleValue))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }
            else if (value is List<string> multipleValues)
            {
                if (multipleValues.All(v => check(discordBotUserService.RoleAssociationService, v)))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

            return Task.FromResult(PreconditionResult.FromError("One or more roles do not exist"));
        }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context,
            IParameterInfo parameterInfo, object value,
            IServiceProvider services)
        {
            return CheckRequirementsAsyncCore(value, services,
                (roleAssociationService, valueStr) => roleAssociationService._allRolesByName.ContainsKey(valueStr));
        }
    }

    public sealed class RoleAssociationPrecondition : BaseRolePrecondition
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context,
            IParameterInfo parameterInfo, object value,
            IServiceProvider services)
        {
            return CheckRequirementsAsyncCore(value, services, (roleAssociationService, valueStr) =>
                roleAssociationService._roleAssociationsByName.ContainsKey(valueStr));
        }
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(RoleAssociationService))
        {
            return this;
        }

        return null;
    }

    public (bool requiresMerge, HashSet<SocketRole> firstRoleSet, HashSet<SocketRole> secondRoleSet) RequiresMerge(SocketRole firstRole, SocketRole secondRole)
    {
        if (!_roleAssociations.TryGetValue(firstRole, out HashSet<SocketRole>? firstRoleSet))
        {
            return (false, [], []);
        }

        if (!_roleAssociations.TryGetValue(secondRole, out HashSet<SocketRole>? secondRoleSet))
        {
            return (false, [], []);
        }

        return (!ReferenceEquals(firstRoleSet, secondRoleSet), firstRoleSet, secondRoleSet);
    }
}

internal static class IndexExtensions
{
    public static string ToPrettyString(this int index)
    {
        if (index is >= 11 and <= 19)
        {
            return $"{index}ᵗʰ";
        }

        int lastDigit = index % 10;
        return lastDigit switch
        {
            0 => $"{index}ᵗʰ",
            1 => $"{index}ˢᵗ",
            2 => $"{index}ⁿᵈ",
            3 => $"{index}ʳᵈ",
            _ => $"{index}ᵗʰ",
        };
    }
}
*/
