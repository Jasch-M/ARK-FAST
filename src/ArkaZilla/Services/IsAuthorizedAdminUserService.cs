using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ArkaZilla.Services;

public class IsAuthorizedAdminUserService : PreconditionAttribute
{
    private const ulong JaschId = 557680932463706213;
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        if (context.User.Id == JaschId)
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        if (context.Interaction.GuildId is null)
        {
            return Task.FromResult(PreconditionResult.FromError("This command can only be used in a guild."));
        }

        SocketGuildUser user = (SocketGuildUser)context.User;
        if (user.GuildPermissions.Administrator)
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        return Task.FromResult(PreconditionResult.FromError("You do not have permission to use this command."));
    }
}
