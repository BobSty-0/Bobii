using bobii_rework.SlashCommands.BobiiSlashCommands;
using Discord.Interactions;

namespace bobii_rework.SlashCommands
{
    public class CreatorSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [Group(SlashCommandNames.Creator, "Includes all commands to edit creator channels")]
        public class Creator : InteractionModuleBase<InteractionContext>
        {
            [SlashCommand(SlashCommandNames.Info, "Returns detailed information about a existing creator channels")]
            public async Task Info()
            {
                await new CreatorInfo(Context).Execute();
            }
        }
    }
}
