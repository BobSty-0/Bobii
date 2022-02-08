using Bobii.src.Entities;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TextUtility
{
    class SlashCommands
    {
        #region Embeds
        public static async Task CreateEmbed(SlashCommandParameter parameter)
        {
            if (parameter.SlashCommandData.Options.Count == 0)
            {
                return;
            }

            var embedTitle = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "title").Result.String;
            var embedContent = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "content").Result.String;

            if (embedTitle == null)
            {
                embedTitle = "";
            }
            if (embedContent == null)
            {
                embedContent = "";
            }


            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(CreateEmbed)).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter, embedTitle, 250, "Title", nameof(CreateEmbed)).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter, embedContent, 4000, "Content", nameof(CreateEmbed)).Result)
            {
                return;
            }
            var channel = (ISocketMessageChannel)parameter.Interaction.Channel;
            await channel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(parameter.Interaction, embedContent, embedTitle).Result);
            await parameter.Interaction.DeferAsync();
            await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();

            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "CreateEmbed", parameter, message: "/tucreateembed succesfully used");
        }

        public static async Task EditEmbed(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(EditEmbed)).Result)
            {
                return;
            }

            var embedTitle = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "title").Result.String;
            var embedContent = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "content").Result.String;

            if (embedTitle == null)
            {
                embedTitle = "";
            }
            if (embedContent == null)
            {
                embedContent = "";
            }

            if (parameter.SlashCommandData.Options.Count == 1)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C140", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C140", parameter.Language).Result).Result });
                return;
            }

            var messageId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            if (messageId == Bobii.Helper.GetContent("C138", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C139", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C139", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(EditEmbed), parameter, message: "No messages detected");
                return;
            }

            messageId = messageId.Split(' ')[0];

            if (Bobii.CheckDatas.CheckMessageID(parameter, messageId, nameof(EditEmbed)).Result ||
                Bobii.CheckDatas.CheckIfMessageFromCreateEmbed(parameter, ulong.Parse(messageId), nameof(EditEmbed)).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter, embedTitle, 250, "Title", nameof(EditEmbed)).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter, embedContent, 4000, "Content", nameof(EditEmbed)).Result)
            {
                return;
            }


            // BIG HACK DO NOT TAKE THIS CODE SERIOUSE!!1!
            try
            {
                RestUserMessage restUserMessage = null;
                SocketUserMessage socketUserMessage = null;
                var channel = (SocketTextChannel)parameter.Client.GetChannel(parameter.Interaction.Channel.Id);
                try
                {
                    restUserMessage = (RestUserMessage)channel.GetMessageAsync(ulong.Parse(messageId)).Result;
                }
                catch (Exception)
                {
                    socketUserMessage = (SocketUserMessage)channel.GetMessageAsync(ulong.Parse(messageId)).Result;
                }

                if (restUserMessage != null)
                {
                    var embed = restUserMessage.Embeds.First();
                    if (embedTitle == "" && embedContent != "")
                    {
                        await restUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, embedContent, embed.Title).Result });
                    }

                    if (embedTitle != "" && embedContent == "")
                    {
                        await restUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, embed.Description, embedTitle).Result });
                    }

                    if (embedTitle != "" && embedContent != "")
                    {
                        await restUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, embedContent, embedTitle).Result });
                    }
                }
                else
                {
                    var embed = socketUserMessage.Embeds.First();
                    if (embedTitle == "" && embedContent != "")
                    {
                        await socketUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, embedContent, embed.Title).Result });
                    }

                    if (embedTitle != "" && embedContent == "")
                    {
                        await socketUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, embed.Description, embedTitle).Result });
                    }

                    if (embedTitle != "" && embedContent != "")
                    {
                        await socketUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, embedContent, embedTitle).Result });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            await parameter.Interaction.DeferAsync();
            await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "EditEmbed", parameter, message: "/tueditembed successfully used");
        }
        #endregion
    }
}
