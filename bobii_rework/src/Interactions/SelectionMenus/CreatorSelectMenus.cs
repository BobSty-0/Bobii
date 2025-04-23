using bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus
{
    public class CreatorSelectMenus : InteractionModuleBase<InteractionContext>
    {
        private readonly InteractionService _interactionService;

        public CreatorSelectMenus(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        [ComponentInteraction(SelectMenuCustomIds.CreatorInfo)]
        public async Task CreatorInfo(ulong[] selectedValues)
        {
            await new CreatorInfoSelectMenu(Context, _interactionService ,selectedValues[0]).Execute();
        }
    }
}
