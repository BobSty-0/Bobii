using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bobii.src.Entities;

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

        public static async Task WriteToConsol(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} SCommands   {message}", color);
            await Task.CompletedTask;
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

            switch (parameter.SlashCommandData.Name)
            {
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
                case "fwadd":
                    await FilterWord.SlashCommands.FWAdd(parameter);
                    break;
                case "fwremove":
                    await FilterWord.SlashCommands.FWRemove(parameter);
                    break;
                case "fwupdate":
                    await FilterWord.SlashCommands.FWUpdate(parameter);
                    break;
                case "fwinfo":
                    await FilterWord.SlashCommands.FWInfo(parameter);
                    break;
                case "flinfo":
                    await FilterLink.SlashCommands.FLInfo(parameter);
                    break;
                case "flset":
                    await FilterLink.SlashCommands.FLSet(parameter);
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
                    await TempChannel.SlashCommands.TempName(parameter);
                    break;
                case "tempsize":
                    await TempChannel.SlashCommands.TempSize(parameter);
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
            }
        }
        #endregion
    }
}
