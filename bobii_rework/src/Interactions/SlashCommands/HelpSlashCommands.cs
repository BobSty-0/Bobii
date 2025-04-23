using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class HelpSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [SlashCommand(SlashCommandNames.Help, "Get information about Bobii")]
        public async Task BobiiHelp()
        {
            await new HelpCommand(Context).Execute();
        }
    }
}
