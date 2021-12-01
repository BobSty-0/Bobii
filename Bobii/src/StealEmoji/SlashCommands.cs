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
        public static async Task StealEmoji(SlashCommandParameter parameter)
        {
            var emoteString = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            var emoteName = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, 
                    "StealEmote").Result ||
                Bobii.CheckDatas.CheckStringLength(parameter.Interaction, parameter.Guild, emoteName, 20, "the emote name", "StealEmote").Result ||
                Bobii.CheckDatas.CheckMinLength(parameter.Interaction, parameter.Guild, emoteName, 2, "the emote name", "StealEmote").Result ||
                Bobii.CheckDatas.CheckStringForAlphanumericCharacters(parameter.Interaction, parameter.Guild, emoteName, "StealEmote").Result)
            {
                return;
            }

            if (!Emote.TryParse(emoteString, out var emote))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "The given Emote is not a " +
                    "valid emote. Make sure to write the emote from discord directly:\nType :name [...] and press [tab]", "Invalid Emote!").Result }, ephemeral: true);
                await Bobii.Helper.WriteToConsol("SlashComms", true, "StealEmoji", parameter, emojiString: emoteString, message: "Failed to convert " +
                    "emote string to emote");
                return;
            }

            if (parameter.Guild.Emotes.Where(e => e.Name == emoteName).FirstOrDefault() != null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "The given name is already " +
                    "used for another emoji in your server!\nPlease make sure to use a unique name!", "Name already used!").Result }, ephemeral: true);
                await Bobii.Helper.WriteToConsol("SlashComms", true, "StealEmoji", parameter, emojiString: emoteString, message: "Emote name already exists");
                return;
            }

            try
            {
                var exepath = AppDomain.CurrentDomain.BaseDirectory;
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(emote.Url), @$"{exepath}\{emoteName}.png");
                }

                using (var stream = File.Open(@$"{exepath}\{emoteName}.png", FileMode.Open))
                {
                    await parameter.Guild.CreateEmoteAsync(emoteName, new Image(stream));
                }

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Emoji was sucessfully " +
                    $"added you should be able to use `:{emoteName}:` now :)", "Successfully added!").Result });
                await Bobii.Helper.WriteToConsol("SlashComms", false, "StealEmoji", parameter, emojiString: emoteString, message: "Sucessfully added Emoji");
                File.Delete($@"C:\Users\geige\Documents\temp\{emoteName}.png");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Emoji could not be added", 
                    "Error!").Result }, ephemeral: true);
                await Bobii.Helper.WriteToConsol("SlashComms", true, "StealEmoji", parameter, emojiString: $"{emote} / {emoteString}", message: "Failed to add Emoji", exceptionMessage: ex.Message);
            }
        }
    }
}
