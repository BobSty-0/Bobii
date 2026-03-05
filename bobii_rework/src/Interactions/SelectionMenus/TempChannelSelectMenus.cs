using bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus
{
    public class TempChannelSelectMenus : InteractionModuleBase<InteractionContext>
    {
        [ComponentInteraction(SelectMenuCustomIds.TempChannelGiveOwner)]
        public async Task GiveOwner(IGuildUser[] users)
        {
            await new TempGiveOwnerSelectMenu(Context, users[0]).Execute();
        }

        [ComponentInteraction(SelectMenuCustomIds.TempChannelPrivacy)]
        public async Task Privacy(string value)
        {
            // TODO irgendwie herausfinden wie ich die aufrufende elemente löschen kann
            // TODO schauen dass alle UsedFunctions nach dem löschen vom Voice Channel gelöscht werden

            await new TempPrivacySelectionMenu(Context, value).Execute();
        }
    }
}
