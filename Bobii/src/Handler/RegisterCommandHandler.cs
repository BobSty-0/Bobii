using Bobii.src.Models;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class RegisterHandlingService
    {
        public static async Task CommandRegisteredRespond(SlashCommandParameter parameter, string commandName)
        {
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                String.Format(Bobii.Helper.GetContent("C023", parameter.Language).Result, commandName, parameter.GuildUser.Username),
                Bobii.Helper.GetCaption("C023", parameter.Language).Result).Result });

            await Handler.HandlingService._bobiiHelper.WriteToConsol("RegistComs", false, "ComRegister", message: $"/comregister <{commandName}> successfully used");
        }

        public static async Task CommandRegisteredErrorRespond(SlashCommandParameter parameter, string commandName, string exMessage)
        {
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                String.Format(Bobii.Helper.GetContent("C024", parameter.Language).Result, commandName),
                Bobii.Helper.GetCaption("C024", parameter.Language).Result).Result }, ephemeral: true);

            await Handler.HandlingService._bobiiHelper.WriteToConsol("RegistComs", true, "ComRegister", message: $"/comregister <{commandName}> failed to register", exceptionMessage: exMessage);
        }

        public static async Task HandleRegisterCommands(SlashCommandParameter parameter, string commandName)
        {
            try
            {
                switch (commandName)
                {
                    case "test":
                        await Bobii.RegisterCommands.Test(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "help":
                        await Bobii.RegisterCommands.Help(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tcinfo":
                        await TempChannel.RegisterCommands.Info(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tcadd":
                        await TempChannel.RegisterCommands.Add(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tcremove":
                        await TempChannel.RegisterCommands.Remove(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tcupdate":
                        await TempChannel.RegisterCommands.Update(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "comdelete":
                        await ComEdit.RegisterCommands.Delete(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "comdeleteguild":
                        await ComEdit.RegisterCommands.GuildDelete(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "comregister":
                        await ComEdit.RegisterCommands.Register(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "flinfo":
                        await FilterLink.RegisterCommands.Info(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "flguildinfo":
                        await FilterLink.RegisterCommands.GuildInfo(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "flset":
                        await FilterLink.RegisterCommands.Set(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "flcreate":
                        await FilterLink.RegisterCommands.Create(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "fldelete":
                        await FilterLink.RegisterCommands.Delete(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "flladd":
                        await FilterLink.RegisterCommands.LinkAdd(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "fllremove":
                        await FilterLink.RegisterCommands.LinkRemove(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "fluadd":
                        await FilterLink.RegisterCommands.UserAdd(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "fluremove":
                        await FilterLink.RegisterCommands.UserRemove(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "logset":
                        await FilterLink.RegisterCommands.LogSet(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "logupdate":
                        await FilterLink.RegisterCommands.LogUpdate(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "logremove":
                        await FilterLink.RegisterCommands.LogRemove(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "servercount":
                        await Bobii.RegisterCommands.SerververCount(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "refresh":
                        await Bobii.RegisterCommands.Refresh(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tucreateembed":
                        await TextUtility.RegisterCommands.CreateEmbed(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tueditembed":
                        await TextUtility.RegisterCommands.EditEmbed(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tempname":
                        await TempChannel.RegisterCommands.Name(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tempsize":
                        await TempChannel.RegisterCommands.Size(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tempowner":
                        await TempChannel.RegisterCommands.Owner(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tempkick":
                        await TempChannel.RegisterCommands.Kick(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tempblock":
                        await TempChannel.RegisterCommands.Block(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tempunblock":
                        await TempChannel.RegisterCommands.UnBlock(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "temphide":
                        await TempChannel.RegisterCommands.Hide(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tempunhide":
                        await TempChannel.RegisterCommands.UnHide(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "templock":
                        await TempChannel.RegisterCommands.Lock(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tempunlock":
                        await TempChannel.RegisterCommands.UnLock(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "tccreateinfo":
                        await TempChannel.RegisterCommands.CreateInfoForTempCommands(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "stealemoji":
                        await StealEmoji.RegisterCommands.StealEmoji(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "stealemojiurl":
                        await StealEmoji.RegisterCommands.StealEmojiUrl(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    case "backup":
                        await Bobii.RegisterCommands.Backup(parameter.Client);
                        await CommandRegisteredRespond(parameter, commandName);
                        break;
                    default:
                        await CommandRegisteredErrorRespond(parameter, commandName, $"There is no command with the name {commandName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                await CommandRegisteredErrorRespond(parameter, commandName, ex.Message);
            }
        }
    }
}
