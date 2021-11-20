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
            var title = "";
            var content = "";
            if (parameter.SlashCommandData.Options.Count == 0)
            {
                return;
            }

            var parameterName = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Name.ToString();
            if (parameter.SlashCommandData.Options.Count == 1)
            {
                if (parameterName == "title")
                {
                    title = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
                }
                else
                {
                    content = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
                }
            }
            else
            {
                if (parameterName == "title")
                {
                    title = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
                    content = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();
                }
                else
                {
                    content = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
                    title = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();
                }
                
            }


            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "tucreateembed").Result ||
                Bobii.CheckDatas.CheckStringLength(parameter.Interaction, parameter.Guild, title, 250, "Title", "CreateEmbed").Result ||
                Bobii.CheckDatas.CheckStringLength(parameter.Interaction, parameter.Guild, title, 4000, "Content", "CreateEmbed").Result)
            {
                return;
            }
            var channel = (ISocketMessageChannel)parameter.Interaction.Channel;
            await channel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(parameter.Interaction, content, title).Result);
            await parameter.Interaction.DeferAsync();
            await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();

            await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: CreateEmbed | Guild: {parameter.GuildID} | /tucreateembed succesfully used");
        }

        public static async Task EditEmbed(SlashCommandParameter parameter)
        {
            var title = "";
            var content = "";
            if (parameter.SlashCommandData.Options.Count == 1)
            {
                return;
            }
            var parameterName = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Name.ToString();
            if (parameter.SlashCommandData.Options.Count == 2)
            {
                if (parameterName == "title")
                {
                    title = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();
                }
                else
                {
                    content = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();
                }
            }
            else
            {
                if (parameterName  == "title")
                {
                    title = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();
                    content = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[2].Value.ToString();
                }
                else
                {
                    content = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();
                    title = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[2].Value.ToString();
                }
            }

            var messageId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "EditEmbed").Result ||
                Bobii.CheckDatas.CheckMessageID(parameter.Interaction, parameter.Guild, messageId, "EditEmbed", parameter.Client).Result ||
                Bobii.CheckDatas.CheckIfMessageFromCreateEmbed(parameter.Interaction, parameter.Guild, ulong.Parse(messageId), "EditEmbed", parameter.Client).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter.Interaction, parameter.Guild, title, 250, "Title", "EditEmbed").Result ||
                Bobii.CheckDatas.CheckStringLength(parameter.Interaction, parameter.Guild, title, 4000, "Content", "EditEmbed").Result)
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
                    if (parameterName == "")
                    {
                        await restUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, content, title).Result });
                    }
                    else
                    {
                        var embed = restUserMessage.Embeds.First();
                        if (parameterName == "title")
                        {
                            await restUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, embed.Description, title).Result });
                        }
                        else
                        {
                            await restUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, content, embed.Title).Result });
                        }
                    }
                }
                else
                {
                    if (parameterName == "")
                    {
                        await socketUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, content, title).Result });
                    }
                    else
                    {
                        var embed = socketUserMessage.Embeds.First();
                        if (parameterName == "title")
                        {
                            await socketUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, embed.Description, title).Result });
                        }
                        else
                        {
                            await socketUserMessage.ModifyAsync(msg => msg.Embeds = new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, content, embed.Title).Result });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            await parameter.Interaction.DeferAsync();
            await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
            await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: EditEmbed | Guild: {parameter.GuildID} | /tueditembed successfully used");
        }
        #endregion
    }
}
