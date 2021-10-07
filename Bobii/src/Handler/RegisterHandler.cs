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
                        await src.RegisterCommands.Bobii.Help(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcinfo":
                        await src.RegisterCommands.TempChannel.Info(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcadd":
                        await src.RegisterCommands.TempChannel.Add(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcremove":
                        await src.RegisterCommands.TempChannel.Remove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcupdate":
                        await src.RegisterCommands.TempChannel.Update(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comdelete":
                        await src.RegisterCommands.ComEdit.Delete(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comdeleteguild":
                        await src.RegisterCommands.ComEdit.GuildDelete(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comregister":
                        await src.RegisterCommands.ComEdit.Register(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwadd":
                        await src.RegisterCommands.FilterWord.Add(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwremove":
                        await src.RegisterCommands.FilterWord.Remove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwupdate":
                        await src.RegisterCommands.FilterWord.Update(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwinfo":
                        await src.RegisterCommands.FilterWord.Info(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "bobiiguides":
                        await src.RegisterCommands.Bobii.Guides(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flinfo":
                        await src.RegisterCommands.FilterLink.Info(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flset":
                        await src.RegisterCommands.FilterLink.Set(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flladd":
                        await src.RegisterCommands.FilterLink.LinkAdd(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fllremove":
                        await src.RegisterCommands.FilterLink.LinkRemove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fluadd":
                        await src.RegisterCommands.FilterLink.UserAdd(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fluremove":
                        await src.RegisterCommands.FilterLink.UserRemove(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logset":
                        await src.RegisterCommands.FilterLink.LogSet(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logupdate":
                        await src.RegisterCommands.FilterLink.LogUpdate(client);
                        CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logremove":
                        await src.RegisterCommands.FilterLink.LogRemove(client);
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
