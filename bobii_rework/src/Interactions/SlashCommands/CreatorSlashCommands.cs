using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class CreatorSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [SlashCommand(SlashCommandNames.Setup, "Sets an creator channel up")]
        public async Task CreatorSetup()
        {
            await new CreatorSetupCommand(Context).Execute();
        }
    }
}
