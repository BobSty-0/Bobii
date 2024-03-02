using Bobii.src.AutocompleteHandler;
using Bobii.src.Bobii;
using Bobii.src.Helper;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bobii.src.InteractionModules.Slashcommands
{
    class LanguageShlashCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        [SlashCommand("language", "Changes the language of Bobii responses")]
        public async Task BobiiLanguage(
             [Summary("language", "Choose the language which you want to use")][Autocomplete(typeof(LanguagesHandler))] string language)
        {
            var parameter = Context.ContextToParameter();
            try
            {
                var languages = new List<string>()
                {
                    "de",
                    "en",
                    "ru"
                };

                if (!languages.Contains(language))
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C194", parameter.Language).Result,
                             GeneralHelper.GetCaption("C194", parameter.Language).Result).Result }, ephemeral: true);


                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(BobiiLanguage), parameter, message: "/language - invalid language");
                    return;
                }

                if (CheckDatas.CheckUserPermission(parameter, nameof(BobiiLanguage)).Result)
                {
                    return;
                }

                if (!LanguageHelper.LanguageExistiert(parameter.GuildID).Result)
                {
                    await LanguageHelper.AddLanguage(parameter.GuildID, language);
                }
                else
                {
                    await LanguageHelper.UpdateLanguage(parameter.GuildID, language);
                }

                parameter.Language = language;

                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             string.Format(GeneralHelper.GetContent("C195", parameter.Language).Result, language),
                             GeneralHelper.GetCaption("C195", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(BobiiLanguage), parameter,
                    message: $"/language - {language} updated");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

