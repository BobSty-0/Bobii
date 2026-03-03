using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TestCommand(InteractionContext context) : BobiiInteractionBase(context, false)
    {
        public override async Task ExecuteCommand()
        {
        }
    }
}
