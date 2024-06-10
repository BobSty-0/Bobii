using bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus
{
    public class CreatorInfoSelectionMenus : InteractionModuleBase<InteractionContext>
    {
        private readonly InteractionService _interactionService;

        public CreatorInfoSelectionMenus(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        [ComponentInteraction(SelectMenuCustomIds.CreatorInfo)]
        public async Task CreatorInfo(ulong[] selectedValues)
        {
            await new CreatorInfoSelectionMenu(Context, _interactionService ,selectedValues[0]).Execute();
        }
    }
}
