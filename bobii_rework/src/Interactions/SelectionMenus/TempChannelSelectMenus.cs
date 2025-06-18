using bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord;
using Discord.Interactions;

namespace bobii_rework.src.Interactions.SelectionMenus
{
    public class TempChannelSelectMenus : InteractionModuleBase<InteractionContext>
    {
        [ComponentInteraction(SelectMenuCustomIds.TempChannelGiveOwner)]
        public async Task GiveOwner(IGuildUser[] users)
        {
            await new TempGiveOwnerSelectMenu(Context, users[0]).Execute();
        }
    }
}
