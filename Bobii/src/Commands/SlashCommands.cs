using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bobii.src.Commands
{
    class SlashCommands
    {
        public static async Task SlashCommandHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parsedArg = (SocketSlashCommand)interaction;
            var user = (SocketGuildUser)parsedArg.User;
            var guildID = TextChannel.TextChannel.GetGuildWithInteraction(interaction).Id.ToString();
            var guild = TextChannel.TextChannel.GetGuildWithInteraction(interaction);

            switch (parsedArg.Data.Name)
            {
                case "tempinfo":
                    await interaction.RespondAsync("", false, TempVoiceChannel.TempVoiceChannel.CreateVoiceChatInfoEmbed(guildID, client, interaction));
                    await WriteToConsol($"Information: | Task: TempInfo | Guild: {guildID} | /tempinfo successfully used");
                    break;
                case "help":
                    await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateHelpInfoEmbed(guildID, interaction, client));
                    await WriteToConsol($"Information: | Task: Help | Guild: {guildID} | /help successfully used");
                    break;
                case "tempadd":
                    await TempAdd(parsedArg, interaction, guildID, guild, user);
                    break;
                case "tempremove":
                    await TempRemove(parsedArg, interaction, guildID, guild, user);
                    break;
                case "tempchangename":
                    await TempChangeName(parsedArg, interaction, guildID, user);
                    break;
                case "removecommand":
                    await ComDelete(parsedArg, interaction, guildID, user, client);
                    break;
            }
        }



        private static List<SocketSlashCommandDataOption> GetOptions(IReadOnlyCollection<SocketSlashCommandDataOption> options)
        {
            var optionList = new List<SocketSlashCommandDataOption>();
            foreach (var option in options)
            {
                optionList.Add(option);
            }
            return optionList;
        }

        #region CheckData
        private static bool CheckCreateChannelID(SocketInteraction interaction, string Id, string guildID, string task)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(Id, out _) && Id.Length != 18)
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The given channel ID **'{Id}'** is not valid!\nMake sure to copy the ID from the voice channel directly!", "Invalid ID!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {Id} | Invalid ID");
                return true;
            }
            return false;
        }

        private static bool CheckDoubleCreateTempChannel(SocketInteraction interaction, string createChannelID, string guildID, string task)
        {
            if (DBStuff.createtempchannels.CheckIfCreateVoiceChannelExist(guildID, createChannelID))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel with the ID **'{createChannelID}'** already exists!\nYou can get a list of all CreateTempChannels by using:\n**/tempinfo**", "CreateTempChannel exists already!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | Double CreateTempChannel");
                return true;
            }
            return false;
        }

        private static bool CheckIfCreateTempChannelExists(SocketInteraction interaction, string createChannelID, string guildID, string task)
        {
            if (!DBStuff.createtempchannels.CheckIfCreateVoiceChannelExist(guildID, createChannelID))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel with the ID **'{createChannelID}'** does not exists!\nYou can get a list of all CreateTempChannels by using:\n**/tempinfo**", "CreateTempChannel does not exist!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | CreateTempChannel does not exist");
                return true;
            }
            return false;
        }

        private static bool CheckNameLength(SocketInteraction interaction, string createChannelID, string guildID, string name, string task)
        {
            if (name.Length > 50)
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The TempChannelName **'{name}'** has more than 50 characters, pls make sure the name is shorter than 50 characters!", "Too much characters!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | TempChannelName: {name} | Name has too much characters");
                return true;
            }
            return false;
        }

        private static bool CheckUserPermission(SocketInteraction interaction, string guildID, SocketGuildUser user, SocketSlashCommand parsedArg, string task)
        {
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild)
            {
                return false;
            }
            interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Data.Name}**\nYou need one fo the following permissions to use this command:\n**Administrator**\n**Manage Server**", "Missing permissions!"));
            WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | User: {user}| Missing premissions");
            return true;
        }

        private static bool CheckIfItsBobSty(SocketInteraction interaction, string guildID, SocketGuildUser user, SocketSlashCommand parsedArg, string task)
        {
            //False = Its me
            //True = Its not me
            if (user.Id.ToString() == (410312323409117185).ToString())
            {
                return false;
            }
            interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Data.Name}**\n**__Only BobSty himselfe is allowed to use this command!__**", "Missing permissions!"));
            WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | User: {user} | Tryed to delete command: {GetOptions(parsedArg.Data.Options)[0].Value} | Someone tryed to be Me");
            return true;
        }
        #endregion

        #region Tasks   
        public static async Task WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} SCommands   {message}");
            await Task.CompletedTask;
        }

        private static async Task ComDelete(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user, DiscordSocketClient client)
        {
            var delCommand = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var commands = client.Rest.GetGlobalApplicationCommands();

            if(CheckIfItsBobSty(interaction, guildID, user, parsedArg, "ComDelete"))
            {
                return;
            }

            foreach(Discord.Rest.RestGlobalCommand command in commands.Result)
            {
                if(command.Name == delCommand)
                {
                    try
                    {
                        await command.DeleteAsync();
                        await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The command **'/{command.Name}'** was sucessfully deleted by the one and only **{user.Username}**", "Command successfully deleted"));
                        await WriteToConsol($"Information: | Task: ComDelete | Guild: {guildID} | Command: /{command.Name} | User: {user} | /comdelete successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command **'/{command.Name}'** could not be removed", "Error!"));
                        await WriteToConsol($"Error: | Task: ComDelete | Guild: {guildID} | Command: /{command.Name} | User: {user} | Failed to delete | {ex.Message}"); 
                        return;
                    }
                }
            }

            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command {delCommand} could not be found!", "Error!"));
            await WriteToConsol($"Error: | Task: ComDelete | Guild: {guildID} | Command: /{delCommand} | User: {user} | No command with this name found");
            return;

        }

        private static async Task TempAdd(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuild guild, SocketGuildUser user)
        {
            var createChannelID = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var name = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "TempAdd") ||
                CheckCreateChannelID(interaction, createChannelID, guildID, "TempAdd") ||
                CheckDoubleCreateTempChannel(interaction, createChannelID, guildID, "TempAdd") ||
                CheckNameLength(interaction, createChannelID, guildID, name, "TempAdd"))
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            name = name.Replace("'", "’");

            try
            {
                DBStuff.createtempchannels.AddCC(guildID, name, createChannelID);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel **'{guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully added by **{user.Username}**", "CreateTempChannel sucessfully added!"));
                await WriteToConsol($"Information: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tempadd successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "CreateTempChannel could not be added", "Error!"));
                await WriteToConsol($"Error: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | Failed to add CreateTempChannel | {ex.Message}");
                return;
            }
        }

        private static async Task TempRemove(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuild guild, SocketGuildUser user)
        {
            var createChannelID = GetOptions(parsedArg.Data.Options)[0].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "TempRemove") ||
                CheckCreateChannelID(interaction, createChannelID, guildID, "TempRemove") ||
                CheckIfCreateTempChannelExists(interaction, createChannelID, guildID, "TempRemove"))
            {
                return;
            }

            try
            {
                DBStuff.createtempchannels.RemoveCC(guildID, createChannelID);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel **'{guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully removed by **{user.Username}**", "Successfully removed!"));
                await WriteToConsol($"Information: | Task: TempRemove | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tempremove successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "CreateTempChannel could not be removed", "Error!"));
                await WriteToConsol($"Error: | Task: TempRemove | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | Failed to remove CreateTempChannel | {ex.Message}");
                return;
            }
        }

        private static async Task TempChangeName(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user)
        {
            var createChannelID = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var voiceNameNew = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "TempChangeName") ||
                CheckCreateChannelID(interaction, createChannelID, guildID, "TempChangeName") ||
                CheckIfCreateTempChannelExists(interaction, createChannelID, guildID, "TempChangeName") ||
                CheckNameLength(interaction, createChannelID, guildID, voiceNameNew, "TempChangeName"))
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            voiceNameNew = voiceNameNew.Replace("'", "’");

            try
            {
                DBStuff.createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"TempChannelName succesfully changed to: **'{voiceNameNew}'**", "Name successfully changed!"));
                await WriteToConsol($"Information: | Task: TempChangeName | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tempchangename successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "TempChannelName could not be changed", "Error!"));
                await WriteToConsol($"Error: | Task: TempChangeName | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | Failed to change TempChannelName | {ex.Message}");
                return;
            }
            DBStuff.createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
            await Task.CompletedTask;
        }
        #endregion
    }
}
