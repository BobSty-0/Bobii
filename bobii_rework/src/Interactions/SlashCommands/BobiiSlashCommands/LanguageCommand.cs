using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class LanguageCommand : BobiiInteractionBase
    {
        #region Declarations
        private Language _language;
        #endregion
        public LanguageCommand(InteractionContext context, Language language) : base(context)
        {
            _language = language;
        }

        public override async Task ExecuteCommand()
        {
            await LanguageRepository.ChangeLanguage(Context.Guild!.Id, _language);
            Context.Language = _language;
            await Context.RespondAsync(Captions.Success, Contents.SpracheErfolgreichGeaendert);
        }

        public override async Task<bool> CheckData()
        {
            return await NotEnoughPermissions();
        }
    }
}
