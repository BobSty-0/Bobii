using Bobii.src.Bobii;
using Bobii.src.Helper;
using Bobii.src.Models;
using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.TextUtility
{
    class Modals
    {
        public static async Task CreateEmbed(SlashCommandParameter parameter)
        {
            var title = parameter.Modal.Data.Components.Where(c => c.CustomId == "title").SingleOrDefault().Value;
            var content = parameter.Modal.Data.Components.Where(c => c.CustomId == "content").SingleOrDefault().Value;
            var imageUrl = parameter.Modal.Data.Components.Where(c => c.CustomId == "imageurl").SingleOrDefault().Value;
            var otherUrl = parameter.Modal.Data.Components.Where(c => c.CustomId == "url").SingleOrDefault().Value;

            if (title == "" && content == "" && imageUrl == "" && otherUrl == "")
            {
                await parameter.Modal.DeferAsync();
                return;
            }

            if (imageUrl != "" && !imageUrl.StartsWith("https://cdn.discordapp.com/") && !imageUrl.EndsWith(".png"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                string.Format(GeneralHelper.GetContent("C229", parameter.Language).Result),
                GeneralHelper.GetCaption("C229", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(CreateEmbed), parameter, parameterName: imageUrl,
                    message: "Not enough caracters");
                return;
            }

            if (otherUrl != "" && !otherUrl.StartsWith("https://"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                string.Format(GeneralHelper.GetContent("C230", parameter.Language).Result),
                GeneralHelper.GetCaption("C229", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(CreateEmbed), parameter, parameterName: imageUrl,
                    message: "Not enough caracters");
                return;
            }

            var channel = parameter.Interaction.Channel;
            await channel.SendMessageAsync(embed: GeneralHelper.CreateTUEmbed(parameter.Guild, content, title, parameter.GuildUser.ToString(), imageUrl, otherUrl).Result);
            await parameter.Interaction.DeferAsync();

            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "CreateEmbed", parameter, message: "/tucreateembed succesfully used");
        }

        public static async Task EditEmbed(SlashCommandParameter parameter, string messageId)
        {
            var title = parameter.Modal.Data.Components.Where(c => c.CustomId == "title").SingleOrDefault().Value;
            var content = parameter.Modal.Data.Components.Where(c => c.CustomId == "content").SingleOrDefault().Value;
            var imageUrl = parameter.Modal.Data.Components.Where(c => c.CustomId == "imageurl").SingleOrDefault().Value;
            var otherUrl = parameter.Modal.Data.Components.Where(c => c.CustomId == "url").SingleOrDefault().Value;

            if (title == "" && content == "" && imageUrl == "" && otherUrl == "")
            {
                await parameter.Modal.DeferAsync();
                return;
            }

            if (imageUrl != "" && !imageUrl.StartsWith("https://cdn.discordapp.com/") && !imageUrl.EndsWith(".png"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                string.Format(GeneralHelper.GetContent("C229", parameter.Language).Result),
                GeneralHelper.GetCaption("C229", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(CreateEmbed), parameter, parameterName: imageUrl,
                    message: "Not enough caracters");
                return;
            }

            if ((otherUrl != "" && !otherUrl.StartsWith("https://")) || (otherUrl != "" && otherUrl.Contains(" ")))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                string.Format(GeneralHelper.GetContent("C230", parameter.Language).Result),
                GeneralHelper.GetCaption("C229", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(CreateEmbed), parameter, parameterName: imageUrl,
                    message: "Not enough caracters");
                return;
            }

            var messages = TextUtilityHelper.GetUserMessages(parameter, ulong.Parse(messageId)).Result;

            try
            {
                if (messages.RestUserMessage != null)
                {
                    await messages.RestUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { GeneralHelper.CreateTUEmbed(parameter.Guild, content, title, parameter.GuildUser.ToString(), imageUrl, otherUrl).Result });
                }
                else
                {
                    await messages.SocketUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { GeneralHelper.CreateTUEmbed(parameter.Guild, content, title, parameter.GuildUser.ToString(), imageUrl, otherUrl).Result });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            await parameter.Interaction.DeferAsync();
            //await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "EditEmbed", parameter, message: "/tueditembed successfully used");
        }
    }
}
