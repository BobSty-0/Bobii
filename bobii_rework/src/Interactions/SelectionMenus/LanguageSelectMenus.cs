using bobii_rework.Extensions;
using bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus
{
    public class LanguageSelectMenus : InteractionModuleBase<InteractionContext>
    {
        [ComponentInteraction(SelectMenuCustomIds.GuildJoinedLanguage)]
        public async Task ChangelLanguage(string[] language)
        {
            await new LanguageSelectMenu(Context, language[0].ToLanguage()).Execute();
        }
    }
}
