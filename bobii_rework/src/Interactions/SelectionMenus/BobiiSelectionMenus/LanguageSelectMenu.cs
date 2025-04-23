using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus
{
    public class LanguageSelectMenu : BobiiInteractionBase
    {
        #region Declarations
        private Language _language;
        #endregion

        #region Contsructor
        public LanguageSelectMenu(InteractionContext context, Language language) : base(context)
        {
            _language = language;
        }
        #endregion

        #region Tasks
        public override async Task ExecuteCommand()
        {
            await LanguageRepository.ChangeLanguage(Context.Guild!.Id, _language);
            await Context.ModifyEmbedAndComponentsFromOriginalResponse(
                await GeneralHelper.GetJoinedGuildText(_language), 
                await GeneralHelper.GetJoinedGuildMessageComponent(_language));
        }

        public override async Task<bool> CheckData()
        {
            return await NotEnoughPermissions();
        }
        #endregion
    }
}
