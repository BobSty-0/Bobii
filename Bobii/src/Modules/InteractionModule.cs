using Discord.Interactions;
using Discord;
using System.Threading.Tasks;

namespace Bobii.src.Modules
{
    public class InteractionModuleBase : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("tcinfo", "")]
        public async Task HandleTestCommand()
        {
            await RespondAsync(Context.User.Username);
        }
    }
}
