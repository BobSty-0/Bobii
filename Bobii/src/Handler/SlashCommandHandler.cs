using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bobii.src.Entities;
using Discord;

namespace Bobii.src.Handler
{
    class SlashCommandHandlingService
    {
        #region Tasks
        public static async Task<List<SocketSlashCommandDataOption>> GetOptions(IReadOnlyCollection<SocketSlashCommandDataOption> options)
        {
            var optionList = new List<SocketSlashCommandDataOption>();
            foreach (var option in options)
            {
                optionList.Add(option);
            }
            await Task.CompletedTask;
            return optionList;
        }

        public static async Task<ApplicationCommandOptionTypes> GetOptionWithName(Entities.SlashCommandParameter parameter, string optionName)
        {
            var applicationCommandOptionType = new ApplicationCommandOptionTypes();
            foreach (var option in parameter.SlashCommandData.Options)
            {
                if (option.Name == optionName)
                {
                    switch (option.Type)
                    {
                        case ApplicationCommandOptionType.Integer:
                            applicationCommandOptionType.Integer = Convert.ToInt32(option.Value);
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.Boolean:
                            applicationCommandOptionType.Boolean = Convert.ToBoolean(option.Value);
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.Channel:
                            applicationCommandOptionType.IGuildChannel = (IGuildChannel)(option.Value);
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.Mentionable:
                            applicationCommandOptionType.IMentionable = (IMentionable)(option.Value);
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.Number:
                            applicationCommandOptionType.Double = Convert.ToDouble(option.Value);
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.Role:
                            applicationCommandOptionType.IRole = (IRole)(option.Value);
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.String:
                            applicationCommandOptionType.String = option.Value.ToString();
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.SubCommand:
                            applicationCommandOptionType.SubCommand = option.Value;
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.SubCommandGroup:
                            applicationCommandOptionType.SubCommandGroup = option.Value;
                            return applicationCommandOptionType;

                        case ApplicationCommandOptionType.User:
                            applicationCommandOptionType.IUser = (IUser)option.Value;
                            return applicationCommandOptionType;
                    }
                }
            }
            await Task.CompletedTask;
            return applicationCommandOptionType;
        }
        #endregion

        #region Handler  
        public static async Task SlashCommandHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parameter = new SlashCommandParameter();
            parameter.SlashCommand = (SocketSlashCommand)interaction;
            parameter.GuildUser = (SocketGuildUser)parameter.SlashCommand.User;
            parameter.Guild = Bobii.Helper.GetGuildWithInteraction(interaction).Result;
            parameter.GuildID = Bobii.Helper.GetGuildWithInteraction(interaction).Result.Id;
            parameter.Interaction = interaction;
            parameter.Client = client;
            parameter.SlashCommandData = parameter.SlashCommand.Data;
            parameter.Language = Bobii.EntityFramework.BobiiHelper.GetLanguage(parameter.Guild.Id).Result;

            switch (parameter.SlashCommandData.Name)
            {
                case "test":
                    await Bobii.SlashCommands.Test(parameter);
                    break;
                case "tcinfo":
                    await TempChannel.SlashCommands.TCInfo(parameter);
                    break;
                case "bobiiguides":
                    await Bobii.SlashCommands.BobiiGuides(parameter);
                    break;
                case "helpbobii":
                    await Bobii.SlashCommands.HelpBobii(parameter);
                    break;
                case "tcadd":
                    await TempChannel.SlashCommands.TCAdd(parameter);
                    break;
                case "tcremove":
                    await TempChannel.SlashCommands.TCRemove(parameter);
                    break;
                case "tcupdate":
                    await TempChannel.SlashCommands.TCUpdate(parameter);
                    break;
                case "comdelete":
                    await ComEdit.SlashCommands.ComDelete(parameter);
                    break;
                case "comdeleteguild":
                    await ComEdit.SlashCommands.ComDeleteGuild(parameter);
                    break;
                case "comregister":
                    await ComEdit.SlashCommands.ComRegister(parameter);
                    break;
                case "flinfo":
                    await FilterLink.SlashCommands.FLInfo(parameter);
                    break;
                case "flguildinfo":
                    await FilterLink.SlashCommands.FLGuildInfo(parameter);
                    break;
                case "flset":
                    await FilterLink.SlashCommands.FLSet(parameter);
                    break;
                case "flcreate":
                    await FilterLink.SlashCommands.FLCreate(parameter);
                    break;
                case "fldelete":
                    await FilterLink.SlashCommands.FLDelete(parameter);
                    break;
                case "flladd":
                    await FilterLink.SlashCommands.FLLAdd(parameter);
                    break;
                case "fllremove":
                    await FilterLink.SlashCommands.FLLRemove(parameter);
                    break;
                case "fluadd":
                    await FilterLink.SlashCommands.FLUAdd(parameter);
                    break;
                case "fluremove":
                    await FilterLink.SlashCommands.FLURemove(parameter);
                    break;
                case "logset":
                    await FilterLink.SlashCommands.LogSet(parameter);
                    break;
                case "logupdate":
                    await FilterLink.SlashCommands.LogUpdate(parameter);
                    break;
                case "logremove":
                    await FilterLink.SlashCommands.LogRemove(parameter);
                    break;
                case "refresh":
                    await Bobii.SlashCommands.Refresh(parameter);
                    break;
                case "leaveguild":
                    await Bobii.SlashCommands.LeaveGuild(parameter);
                    break;
                case "servercount":
                    await Bobii.SlashCommands.ServerCount(parameter);
                    break;
                case "tucreateembed":
                    await TextUtility.SlashCommands.CreateEmbed(parameter);
                    break;
                case "tueditembed":
                    await TextUtility.SlashCommands.EditEmbed(parameter);
                    break;
                case "tempname":
                    _ = TempChannel.SlashCommands.TempName(parameter);
                    break;
                case "tempsize":
                    _ = TempChannel.SlashCommands.TempSize(parameter);
                    break;
                case "tempowner":
                    await TempChannel.SlashCommands.TempOwner(parameter);
                    break;
                case "tempkick":
                    await TempChannel.SlashCommands.TempKick(parameter);
                    break;
                case "templock":
                    await TempChannel.SlashCommands.TempLock(parameter);
                    break;
                case "tempunlock":
                    await TempChannel.SlashCommands.TempUnLock(parameter);
                    break;
                case "tempblock":
                    await TempChannel.SlashCommands.TempBlock(parameter);
                    break;
                case "tempunblock":
                    await TempChannel.SlashCommands.TempUnBlock(parameter);
                    break;
                case "tccreateinfo":
                    await TempChannel.SlashCommands.TCCreateInfo(parameter);
                    break;
                case "stealemoji":
                    await StealEmoji.SlashCommands.StealEmoji(parameter);
                    break;
                case "stealemojiurl":
                    await StealEmoji.SlashCommands.StealEmojiUrl(parameter);
                    break;
            }
        }
        #endregion
    }
}
