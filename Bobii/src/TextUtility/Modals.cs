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

            if (title == "" && content == "")
            {
                await parameter.Modal.DeferAsync();
                return;
            }

            var channel = parameter.Interaction.Channel;
            await channel.SendMessageAsync(embed: Bobii.Helper.CreateTUEmbed(parameter.Guild, content, title, parameter.GuildUser.ToString()).Result);
            await parameter.Interaction.DeferAsync();

            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "CreateEmbed", parameter, message: "/tucreateembed succesfully used");
        }

        public static async Task EditEmbed(SlashCommandParameter parameter, string messageId)
        {
            var title = parameter.Modal.Data.Components.Where(c => c.CustomId == "title").SingleOrDefault().Value;
            var content = parameter.Modal.Data.Components.Where(c => c.CustomId == "content").SingleOrDefault().Value;

            if (title == "" && content == "")
            {
                await parameter.Modal.DeferAsync();
                return;
            }

            var messages = Helper.GetUserMessages(parameter, ulong.Parse(messageId)).Result;

            try
            {
                if (messages.RestUserMessage != null)
                {
                    await messages.RestUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateTUEmbed(parameter.Guild, content, title, parameter.GuildUser.ToString()).Result });
                }
                else
                {
                    await messages.SocketUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateTUEmbed(parameter.Guild, content, title, parameter.GuildUser.ToString()).Result });
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
