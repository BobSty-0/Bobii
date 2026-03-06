using bobii_rework.src.Enums;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TestCommand(InteractionContext context) : BobiiInteractionBase(context, InteractionReactionType.None)
    {
        public override async Task ExecuteCommand()
        {
        }
    }
}
