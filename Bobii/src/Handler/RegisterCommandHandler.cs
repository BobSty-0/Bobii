using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class RegisterHandlingService
    {
        public static async void WriteToConsol(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} RegisterC   {message}", color);
            await Task.CompletedTask;
        }

        public static async void CommandRegisteredRespond(SocketInteraction interaction, string guildid, string commandName, SocketGuildUser user)
        {
            await interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The command **'/{commandName}'** was sucessfully registered by the one and only **{user.Username}**", "Command successfully registered") });
            WriteToConsol($"Information: | Task: ComRegister | Guild: {guildid} | Command: /{commandName} | /comregister successfully used");
        }

        public static async void CommandRegisteredErrorRespond(SocketInteraction interaction, string guildID, string commandName, SocketGuildUser user, string exMessage)
        {
            await interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The command **'/{commandName}'** failed to register", "Command failed to register") }, ephemeral: true);
            WriteToConsol($"Error: | Task: ComRegister | Guild: {guildID} | Command: /{commandName} | Failed to register | {exMessage}");
        }

        public static async Task HandleRegisterCommands(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user,  string commandName, DiscordSocketClient client)
        {
            try
            {
                switch (commandName)
                {
                    case "helpbobii":
                        await Bobii.RegisterCommands.Help(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcinfo":
                        await TempChannel.RegisterCommands.Info(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcadd":
                        await TempChannel.RegisterCommands.Add(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcremove":
                        await TempChannel.RegisterCommands.Remove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcupdate":
                        await TempChannel.RegisterCommands.Update(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comdelete":
                        await ComEdit.RegisterCommands.Delete(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comdeleteguild":
                        await ComEdit.RegisterCommands.GuildDelete(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comregister":
                        await ComEdit.RegisterCommands.Register(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwadd":
                        await FilterWord.RegisterCommands.Add(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwremove":
                        await FilterWord.RegisterCommands.Remove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwupdate":
                        await FilterWord.RegisterCommands.Update(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwinfo":
                        await FilterWord.RegisterCommands.Info(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "bobiiguides":
                        await Bobii.RegisterCommands.Guides(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flinfo":
                        await FilterLink.RegisterCommands.Info(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flset":
                        await FilterLink.RegisterCommands.Set(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flladd":
                        await FilterLink.RegisterCommands.LinkAdd(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fllremove":
                        await FilterLink.RegisterCommands.LinkRemove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fluadd":
                        await FilterLink.RegisterCommands.UserAdd(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fluremove":
                        await FilterLink.RegisterCommands.UserRemove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logset":
                        await FilterLink.RegisterCommands.LogSet(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logupdate":
                        await FilterLink.RegisterCommands.LogUpdate(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logremove":
                        await FilterLink.RegisterCommands.LogRemove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                }
            }
            catch (Exception ex)
            {
                CommandRegisteredErrorRespond(interaction, guild.Id.ToString(), commandName, user, ex.Message);
            }
        }
    }
}
