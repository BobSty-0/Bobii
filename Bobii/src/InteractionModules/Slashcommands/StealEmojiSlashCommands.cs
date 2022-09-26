using Bobii.src.Bobii;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.Slashcommands
{
    class StealEmojiSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("steal", "Includes all commands to steal emojis")]
        public class Steal : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("emoji", "Adds the used emoji to your server")]
            public async Task StealEmoji(
                [Summary("emoji", "Use the emoji which you want to add to your server")] string emotestring,
                [Summary("name", "This will be the name of your emoji in your server")] string emojiname)
            {
                var parameter = Context.ContextToParameter();

                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(StealEmoji)).Result ||
                    Bobii.CheckDatas.CheckStringLength(parameter, emojiname, 20, "the emote name", nameof(StealEmoji)).Result ||
                    Bobii.CheckDatas.CheckMinLength(parameter, emojiname, 2, "the emote name", nameof(StealEmoji)).Result ||
                    Bobii.CheckDatas.CheckStringForAlphanumericCharacters(parameter, emojiname, nameof(StealEmoji)).Result ||
                    // todo emote hier
                    Bobii.CheckDatas.CheckIfItsAEmoji(parameter, emotestring, nameof(StealEmoji)).Result ||
                    Bobii.CheckDatas.CheckIfEmojiWithNameAlreadyExists(parameter, emojiname, nameof(StealEmoji)).Result)
                {
                    return;
                }

                try
                {
                    var exepath = AppDomain.CurrentDomain.BaseDirectory;
                    using (WebClient client = new WebClient())
                    {
                        // todo
                        client.DownloadFile(new Uri(Emote.Parse(emotestring).Url), @$"{exepath}\{emojiname}.png");
                    }

                    using (var stream = File.Open(@$"{exepath}\{emojiname}.png", FileMode.Open))
                    {
                        await parameter.Guild.CreateEmoteAsync(emojiname, new Image(stream));
                    }

                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C090", parameter.Language).Result, emojiname),
                    Bobii.Helper.GetCaption("C090", parameter.Language).Result).Result });
                    //todo
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(StealEmoji), parameter, emojiString: emotestring, message: "Sucessfully added Emoji");
                    File.Delete($@"{exepath}\{emojiname}.png");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                      Bobii.Helper.GetContent("C091", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    // todo
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(StealEmoji), parameter, emojiString: emotestring, message: "Failed to add Emoji", exceptionMessage: ex.Message);
                }
            }

            [SlashCommand("emojiurl", "Adds the emoji of the given url to your server")]
            public  async Task StealEmojiUrl(
                 [Summary("emojiurl", "Use the emoji url of the emoji which you want to add to your server")] string emojiurl,
                [Summary("name", "This will be the name of your emoji in your server")] string emojiname)
            {
                var parameter = Context.ContextToParameter();

                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(StealEmojiUrl)).Result ||
                    Bobii.CheckDatas.CheckStringLength(parameter, emojiname, 20, "the emote name", nameof(StealEmojiUrl)).Result ||
                    Bobii.CheckDatas.CheckMinLength(parameter, emojiname, 2, "the emote name", nameof(StealEmojiUrl)).Result ||
                    Bobii.CheckDatas.CheckStringForAlphanumericCharacters(parameter, emojiname, nameof(StealEmojiUrl)).Result ||
                    Bobii.CheckDatas.CheckIfLinkIsEmojiLink(parameter, emojiurl, nameof(StealEmojiUrl)).Result ||
                    Bobii.CheckDatas.CheckIfEmojiWithNameAlreadyExists(parameter, emojiname, nameof(StealEmojiUrl)).Result)
                {
                    return;
                }

                try
                {
                    var exepath = AppDomain.CurrentDomain.BaseDirectory;
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(emojiurl), @$"{exepath}\{emojiname}.png");
                    }

                    using (var stream = File.Open(@$"{exepath}\{emojiname}.png", FileMode.Open))
                    {
                        await parameter.Guild.CreateEmoteAsync(emojiname, new Image(stream));
                    }

                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C090", parameter.Language).Result, emojiname),
                    Bobii.Helper.GetCaption("C090", parameter.Language).Result).Result });
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "StealEmojiUrl", parameter, emojiString: emojiurl, message: "Sucessfully added Emoji");
                    File.Delete($@"{exepath}\{emojiname}.png");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C091", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "StealEmojiUrl", parameter, emojiString: emojiurl, message: "Failed to add Emoji", exceptionMessage: ex.Message);
                }
            }
        }
    }
}
