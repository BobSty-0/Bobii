using bobii_rework.Enums;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class LanguageCommand(InteractionContext context, Language language) : BobiiInteractionBase(context, ResponseType.Modify)
    {
        public override async Task ExecuteCommand()
        {
            await LanguageRepository.ChangeLanguage(Context.Guild!.Id, language);
            Context.Language = language;
            await Context.ModifyOriginalResponse(Captions.Success, Contents.SpracheErfolgreichGeaendert);
        }

        public override async Task<bool> CheckData()
        {
            return await NotEnoughPermissions();
        }
    }
}
