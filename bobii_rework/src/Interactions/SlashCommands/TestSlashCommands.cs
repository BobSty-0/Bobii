using bobii_rework.Extensions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class TestSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [SlashCommand("test", "das ist nicht gut")]
        private async Task Test()
        {
            await Context.Interaction.DeferAsync();
            await new TestCommand(Context).Execute();
        }
    }
}
