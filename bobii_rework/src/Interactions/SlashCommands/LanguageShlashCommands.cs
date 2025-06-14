using bobii_rework.Enums;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class LanguageShlashCommands : InteractionModuleBase<InteractionContext>
    {
        [SlashCommand(SlashCommandNames.Language, "Changes the Language of Bobii responses")]
        public async Task BobiiLanguage(
            [Summary("Language", "Choose the Language which you want to use")] Language language)
        {
            await new LanguageCommand(Context, language).Execute();
        }
    }
}
