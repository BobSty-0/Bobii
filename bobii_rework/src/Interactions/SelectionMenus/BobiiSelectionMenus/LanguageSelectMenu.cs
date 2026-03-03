using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.Helper;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus
{
    public class LanguageSelectMenu(InteractionContext context, Language language) : BobiiInteractionBase(context, false)
    {
        #region Tasks
        public override async Task ExecuteCommand()
        {
            await LanguageRepository.ChangeLanguage(Context.Guild!.Id, language);
            await Context.ModifyEmbedAndComponentsFromOriginalResponse(
                await GeneralHelper.GetJoinedGuildText(language),
                await MessageComponentHelper.GetJoinedGuildMessageComponent(language));
        }

        public override async Task<bool> CheckData()
        {
            return await NotEnoughPermissions();
        }
        #endregion
    }
}
