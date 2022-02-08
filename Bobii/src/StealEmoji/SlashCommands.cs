using Bobii.src.Entities;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.StealEmoji
{
    class SlashCommands
    {
        public static async Task StealEmojiUrl(SlashCommandParameter parameter)
        {
            var emoteUrl = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "emojiurl").Result.String;
            var emoteName = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "emojiname").Result.String;

            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(StealEmojiUrl)).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter, emoteName, 20, "the emote name", nameof(StealEmojiUrl)).Result ||
                Bobii.CheckDatas.CheckMinLength(parameter, emoteName, 2, "the emote name", nameof(StealEmojiUrl)).Result ||
                Bobii.CheckDatas.CheckStringForAlphanumericCharacters(parameter, emoteName, nameof(StealEmojiUrl)).Result ||
                Bobii.CheckDatas.CheckIfLinkIsEmojiLink(parameter, emoteUrl, nameof(StealEmojiUrl)).Result ||
                Bobii.CheckDatas.CheckIfEmojiWithNameAlreadyExists(parameter, emoteName, nameof(StealEmojiUrl)).Result)
            {
                return;
            }

            try
            {
                var exepath = AppDomain.CurrentDomain.BaseDirectory;
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(emoteUrl), @$"{exepath}\{emoteName}.png");
                }

                using (var stream = File.Open(@$"{exepath}\{emoteName}.png", FileMode.Open))
                {
                    await parameter.Guild.CreateEmoteAsync(emoteName, new Image(stream));
                }

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                    string.Format(Bobii.Helper.GetContent("C090", parameter.Language).Result, emoteName), 
                    Bobii.Helper.GetCaption("C090", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "StealEmojiUrl", parameter, emojiString: emoteUrl, message: "Sucessfully added Emoji");
                File.Delete($@"{exepath}\{emoteName}.png");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                    Bobii.Helper.GetContent("C091", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "StealEmojiUrl", parameter, emojiString: emoteUrl, message: "Failed to add Emoji", exceptionMessage: ex.Message);
            }
        }

        public static async Task StealEmoji(SlashCommandParameter parameter)
        {
            var emoteString = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "emoji").Result.String;
            var emoteName = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "emojiname").Result.String;

            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(StealEmoji)).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter, emoteName, 20, "the emote name", nameof(StealEmoji)).Result ||
                Bobii.CheckDatas.CheckMinLength(parameter, emoteName, 2, "the emote name", nameof(StealEmoji)).Result ||
                Bobii.CheckDatas.CheckStringForAlphanumericCharacters(parameter, emoteName, nameof(StealEmoji)).Result ||
                Bobii.CheckDatas.CheckIfItsAEmoji(parameter, emoteString, nameof(StealEmoji)).Result ||
                Bobii.CheckDatas.CheckIfEmojiWithNameAlreadyExists(parameter, emoteName, nameof(StealEmojiUrl)).Result)
            {
                return;
            }

            try
            {
                var exepath = AppDomain.CurrentDomain.BaseDirectory;
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(Emote.Parse(emoteString).Url), @$"{exepath}\{emoteName}.png");
                }

                using (var stream = File.Open(@$"{exepath}\{emoteName}.png", FileMode.Open))
                {
                    await parameter.Guild.CreateEmoteAsync(emoteName, new Image(stream));
                }

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C090", parameter.Language).Result, emoteName),
                    Bobii.Helper.GetCaption("C090", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(StealEmoji), parameter, emojiString: emoteString, message: "Sucessfully added Emoji");
                File.Delete($@"{exepath}\{emoteName}.png");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                      Bobii.Helper.GetContent("C091", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(StealEmoji), parameter, emojiString: emoteString, message: "Failed to add Emoji", exceptionMessage: ex.Message);
            }
        }
    }
}
