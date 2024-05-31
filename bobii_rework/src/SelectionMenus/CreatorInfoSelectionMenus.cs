using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.SelectionMenus
{
    public class CreatorInfoSelectionMenus : InteractionModuleBase<InteractionContext>
    {
        [ComponentInteraction(SelectMenuCustomIds.CreatorInfo)]
        public async Task CreatorInfo(ulong id)
        {
            var test = "";
        }
    }
}
