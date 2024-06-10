using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class CreatorSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [Group(SlashCommandNames.Creator, "Includes all commands to edit creator channels")]
        public class Creator : InteractionModuleBase<InteractionContext>
        {
            [SlashCommand(SlashCommandNames.Info, "Returns detailed information about a existing creator channel")]
            public async Task Info()
            {
                await new CreatorInfoCommand(Context).Execute();
            }

            [SlashCommand("setup", "Sets an creator channel up")]
            public async Task CreatorSetup()
            {
                await new CreatorSetupCommand(Context).Execute();
            }
        }
    }
}
