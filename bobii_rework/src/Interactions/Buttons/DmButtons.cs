using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.Buttons.BobiiButtons;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using Discord.Interactions;

namespace bobii_rework.Interactions.Buttons
{
    public class DmButtons : InteractionModuleBase<InteractionContext>
    {
        [ComponentInteraction(ButtonCustomIds.DmDelete)]
        public async Task DeleteMessage()
        {
            await new DmDeleteButton(Context).Execute();
        }
    }
}
