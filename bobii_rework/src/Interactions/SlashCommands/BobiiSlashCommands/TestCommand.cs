using bobii_rework.Extensions;
using bobii_rework.src.Components;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TestCommand : BobiiInteractionBase
    {
        public TestCommand(InteractionContext context) : base(context)
        {
        }

        public override async Task ExecuteCommand()
        {
            var channel = await Context.Guild!.GetChannelAsync(860974744190976023);
            var test = channel.SendInterface(Context, 1248929330696818689);
        }
    }
}
