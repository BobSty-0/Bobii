using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class RegisterHandlingService
    {
        public static async Task CommandRegisteredRespond(SocketInteraction interaction, string guildid, string commandName, SocketGuildUser user)
        {
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The command **'/{commandName}'** was sucessfully registered by the one and only **{user.Username}**", "Command successfully registered").Result });
            await Handler.HandlingService._bobiiHelper.WriteToConsol("RegistComs", false, "ComRegister", message: $"/comregister <{commandName}> successfully used");
        }

        public static async Task CommandRegisteredErrorRespond(SocketInteraction interaction, string guildID, string commandName, SocketGuildUser user, string exMessage)
        {
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The command **'/{commandName}'** failed to register", "Command failed to register").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("RegistComs", true, "ComRegister", message: $"/comregister <{commandName}> failed to register", exceptionMessage: exMessage);
        }

        public static async Task HandleRegisterCommands(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, string commandName, DiscordSocketClient client)
        {
            try
            {
                switch (commandName)
                {
                    case "helpbobii":
                        await Bobii.RegisterCommands.Help(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcinfo":
                        await TempChannel.RegisterCommands.Info(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcadd":
                        await TempChannel.RegisterCommands.Add(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcremove":
                        await TempChannel.RegisterCommands.Remove(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcupdate":
                        await TempChannel.RegisterCommands.Update(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comdelete":
                        await ComEdit.RegisterCommands.Delete(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comdeleteguild":
                        await ComEdit.RegisterCommands.GuildDelete(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comregister":
                        await ComEdit.RegisterCommands.Register(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwadd":
                        await FilterWord.RegisterCommands.Add(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwremove":
                        await FilterWord.RegisterCommands.Remove(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwupdate":
                        await FilterWord.RegisterCommands.Update(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwinfo":
                        await FilterWord.RegisterCommands.Info(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "bobiiguides":
                        await Bobii.RegisterCommands.Guides(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flinfo":
                        await FilterLink.RegisterCommands.Info(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flset":
                        await FilterLink.RegisterCommands.Set(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flladd":
                        await FilterLink.RegisterCommands.LinkAdd(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fllremove":
                        await FilterLink.RegisterCommands.LinkRemove(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fluadd":
                        await FilterLink.RegisterCommands.UserAdd(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fluremove":
                        await FilterLink.RegisterCommands.UserRemove(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logset":
                        await FilterLink.RegisterCommands.LogSet(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logupdate":
                        await FilterLink.RegisterCommands.LogUpdate(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logremove":
                        await FilterLink.RegisterCommands.LogRemove(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "servercount":
                        await Bobii.RegisterCommands.SerververCount(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "refresh":
                        await Bobii.RegisterCommands.Refresh(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tucreateembed":
                        await TextUtility.RegisterCommands.CreateEmbed(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tueditembed":
                        await TextUtility.RegisterCommands.EditEmbed(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tempname":
                        await TempChannel.RegisterCommands.Name(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tempsize":
                        await TempChannel.RegisterCommands.Size(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tempowner":
                        await TempChannel.RegisterCommands.Owner(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tempkick":
                        await TempChannel.RegisterCommands.Kick(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tempblock":
                        await TempChannel.RegisterCommands.Block(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tempunblock":
                        await TempChannel.RegisterCommands.UnBlock(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "templock":
                        await TempChannel.RegisterCommands.Lock(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tempunlock":
                        await TempChannel.RegisterCommands.UnLock(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tccreateinfo":
                        await TempChannel.RegisterCommands.CreateInfoForTempCommands(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "stealemoji":
                        await StealEmoji.RegisterCommands.StealEmoji(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "stealemojiurl":
                        await StealEmoji.RegisterCommands.StealEmojiUrl(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "w2gstart":
                        await Watch2Gether.RegisterCommands.W2GStart(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "w2g":
                        await Watch2Gether.RegisterCommands.W2G(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                }
            }
            catch (Exception ex)
            {
                await CommandRegisteredErrorRespond(interaction, guild.Id.ToString(), commandName, user, ex.Message);
            }
        }
    }
}
