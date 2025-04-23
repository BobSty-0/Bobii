using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Interactions.Buttons
{
    public class CreatorButtons : InteractionModuleBase<InteractionContext>
    {
        [ComponentInteraction(ButtonCustomIds.SetupCreatorChannel)]
        public async Task CreatorSetupButton()
        {
            await new CreatorSetupCommand(Context).Execute();
        }
    }
}
