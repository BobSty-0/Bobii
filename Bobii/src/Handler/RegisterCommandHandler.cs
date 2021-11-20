using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class RegisterHandlingService
    {
        public static async Task WriteToConsol(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} RegisterC   {message}", color);
            await Task.CompletedTask;
        }

        public static async Task CommandRegisteredRespond(SocketInteraction interaction, string guildid, string commandName, SocketGuildUser user)
        {
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The command **'/{commandName}'** was sucessfully registered by the one and only **{user.Username}**", "Command successfully registered").Result });
            await WriteToConsol($"Information: | Task: ComRegister | Guild: {guildid} | Command: /{commandName} | /comregister successfully used");
        }

        public static async Task CommandRegisteredErrorRespond(SocketInteraction interaction, string guildID, string commandName, SocketGuildUser user, string exMessage)
        {
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The command **'/{commandName}'** failed to register", "Command failed to register").Result }, ephemeral: true);
            await WriteToConsol($"Error: | Task: ComRegister | Guild: {guildID} | Command: /{commandName} | Failed to register | {exMessage}");
        }

        public static async Task HandleRegisterCommands(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user,  string commandName, DiscordSocketClient client)
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
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcadd":
                        await TempChannel.RegisterCommands.Add(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcremove":
                        await TempChannel.RegisterCommands.Remove(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "tcupdate":
                        await TempChannel.RegisterCommands.Update(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comdelete":
                        await ComEdit.RegisterCommands.Delete(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comdeleteguild":
                        await ComEdit.RegisterCommands.GuildDelete(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "comregister":
                        await ComEdit.RegisterCommands.Register(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwadd":
                        await FilterWord.RegisterCommands.Add(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwremove":
                        await FilterWord.RegisterCommands.Remove(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwupdate":
                        await FilterWord.RegisterCommands.Update(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fwinfo":
                        await FilterWord.RegisterCommands.Info(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "bobiiguides":
                        await Bobii.RegisterCommands.Guides(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flinfo":
                        await FilterLink.RegisterCommands.Info(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flset":
                        await FilterLink.RegisterCommands.Set(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "flladd":
                        await FilterLink.RegisterCommands.LinkAdd(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fllremove":
                        await FilterLink.RegisterCommands.LinkRemove(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fluadd":
                        await FilterLink.RegisterCommands.UserAdd(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "fluremove":
                        await FilterLink.RegisterCommands.UserRemove(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logset":
                        await FilterLink.RegisterCommands.LogSet(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logupdate":
                        await FilterLink.RegisterCommands.LogUpdate(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "logremove":
                        await FilterLink.RegisterCommands.LogRemove(client);
                        await CommandRegisteredRespond (interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "servercount":
                        await Bobii.RegisterCommands.SerververCount(client);
                        await CommandRegisteredRespond(interaction, guild.Id.ToString(), commandName, user);
                        break;
                    case "refresh":
                        await Bobii.RegisterCommands.Refresh(client);
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
