using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bobii.src.DBStuff.Tables;

namespace Bobii.src.Commands
{
    class SlashCommands
    {
        #region Handler  
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
                    WriteToConsol($"Information: | Task: TempInfo | Guild: {guildID} | /tempinfo successfully used");
                    break;
                case "helpbobii":
                    await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateHelpInfoEmbed(guildID, interaction, client));
                    WriteToConsol($"Information: | Task: Help | Guild: {guildID} | /help successfully used");
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
                case "comdelete":
                    await ComDeleteGlobalSlashCommands(parsedArg, interaction, guildID, user, client);
                    break;
                case "comdeleteguild":
                    await ComDeleteGuildSlashCommands(parsedArg, interaction, guildID, user, client);
                    break;
                case "comregister":
                    await ComRegister(parsedArg, interaction, guildID, user, client);
                    break;
                case "badwordadd":
                    await BadWordAdd(parsedArg, interaction, guildID, user);
                    break;
                case "badwordremove":
                    await BadWordRemove(parsedArg, interaction, guildID, user);
                    break;
                case "badwordchangereplaceword":
                    await BadWordChangeReplaceWord(parsedArg, interaction, guildID, user);
                    break;
                case "badwordinfo":
                    await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateNoWordInfoEmbed(interaction, guildID));
                    WriteToConsol($"Information: | Task: BadWordInfo | Guild: {guildID} | /badwordinfo successfully used");
                    break;
            }
        }
        #endregion

        #region Funkitons
        private static List<SocketSlashCommandDataOption> GetOptions(IReadOnlyCollection<SocketSlashCommandDataOption> options)
        {
            var optionList = new List<SocketSlashCommandDataOption>();
            foreach (var option in options)
            {
                optionList.Add(option);
            }
            return optionList;
        }
        #endregion

        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} SCommands   {message}");
            await Task.CompletedTask;
        }

        public static async void CommandRegisteredRespond(SocketInteraction interaction, string guildID, string commandName, SocketGuildUser user)
        {
            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The command **'/{commandName}'** was sucessfully registered by the one and only **{user.Username}**", "Command successfully registered"));
            WriteToConsol($"Information: | Task: ComRegister | Guild: {guildID} | Command: /{commandName} | /comregister successfully used");
        }

        public static async void CommandRegisteredErrorRespond(SocketInteraction interaction, string guildID, string commandName, SocketGuildUser user, string exMessage)
        {
            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The command **'/{commandName}'** failed to register", "Command failed to register"));
            WriteToConsol($"Error: | Task: ComRegister | Guild: {guildID} | Command: /{commandName} | Failed to register | {exMessage}");
        }
        #endregion

        #region CheckData
        private static bool CheckDiscordID(SocketInteraction interaction, string Id, string guildID, string task, bool channel)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(Id, out _) && Id.Length != 18)
            {
                if (channel)
                {
                    interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The given channel ID **'{Id}'** is not valid!\nMake sure to copy the ID from the channel directly!", "Invalid ID!"));
                    WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {Id} | Invalid ID");
                    return true;
                }
                else
                {
                    interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The given guild ID **'{Id}'** is not valid!\nMake sure to copy the ID from the guild directly!", "Invalid ID!"));
                    WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | GuildID: {Id} | Invalid ID");
                    return true;
                }

            }
            return false;
        }

        private static bool CheckDoubleCreateTempChannel(SocketInteraction interaction, string createChannelID, string guildID, string task)
        {
            if (createtempchannels.CheckIfCreateVoiceChannelExist(guildID, createChannelID))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel with the ID **'{createChannelID}'** already exists!\nYou can get a list of all CreateTempChannels by using:\n**/tempinfo**", "CreateTempChannel exists already!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | Double CreateTempChannel");
                return true;
            }
            return false;
        }

        private static bool CheckIfCreateTempChannelExists(SocketInteraction interaction, string createChannelID, string guildID, string task)
        {
            if (!createtempchannels.CheckIfCreateVoiceChannelExist(guildID, createChannelID))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel with the ID **'{createChannelID}'** does not exists!\nYou can get a list of all CreateTempChannels by using:\n**/tempinfo**", "CreateTempChannel does not exist!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | CreateTempChannel does not exist");
                return true;
            }
            return false;
        }

        private static bool CheckIfBadWordDouble(SocketInteraction interaction, string badWord, string guildID, string task)
        {
            if (badwords.CheckIfBadWordExists(guildID, badWord))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The BadWord with the name **'{badWord}'** already exists!\nYou can get a list of all BadWords by using:\n**/badwordinfo**", "BadWord already exists!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | badWord: {badWord} | Double BadWord");
                return true;
            }
            return false;
        }

        private static bool CheckIfBadWordExists(SocketInteraction interaction, string badWord, string guildID, string task)
        {
            if (!badwords.CheckIfBadWordExists(guildID, badWord))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The BadWord with the name **'{badWord}'** does not exists!\nYou can get a list of all BadWords by using:\n**/badwordinfo**", "BadWord does not exist!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | badWord: {badWord} | BadWord does not exist");
                return true;
            }
            return false;
        }

        private static bool CheckNameLength(SocketInteraction interaction, string createChannelID, string guildID, string name, string task, int lenght, bool tempchannel)
        {
            if (name.Length > lenght)
            {
                if (tempchannel)
                {
                    interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The TempChannelName **'{name}'** has more than 50 characters, pls make sure the name is shorter than {lenght} characters!", "Too much characters!"));
                    WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | TempChannelName: {name} | Name has too much characters");
                    return true;
                }
                else
                {
                    interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The the word **'{name}'** has more than {lenght} characters, pls make sure the name is shorter than {lenght} characters!", "Too much characters!"));
                    WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | Name has too much characters");
                    return true;
                }
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

        public static bool CheckIfItsBobSty(SocketInteraction interaction, string guildID, SocketGuildUser user, SocketSlashCommand parsedArg, string task, bool errorMessage)
        {
            //False = Its me
            //True = Its not me
            if (user.Id.ToString() == (410312323409117185).ToString())
            {
                return false;
            }
            if (errorMessage)
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Data.Name}**\n**__Only BobSty himselfe is allowed to use this command!__**", "Missing permissions!"));
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | User: {user} | Tryed to delete command: {GetOptions(parsedArg.Data.Options)[0].Value} | Someone tryed to be Me");
            }
            return true;
        }
        #endregion

        #region Tasks 
        private static async Task BadWordChangeReplaceWord(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user)
        {
            var badword = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var newReplaceWord = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            badword = badword.Replace("'", "’");
            newReplaceWord = newReplaceWord.Replace("'", "’");

            //Check for valid input
            if (CheckNameLength(interaction, "", guildID, newReplaceWord, "BadWordChangeReplaceWord", 20, false) ||
                CheckNameLength(interaction, "", guildID, badword, "BadWordChangeReplaceWord", 20, false) ||
                CheckIfBadWordExists(interaction, badword, guildID, "BadWordChangeReplaceWord"))
            {
                return;
            }
            try
            {
                badwords.ChangeBadWordReplaceWord(badword, newReplaceWord);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The ReplaceWord of the BadWord **'{badword}'** was successfully changed to **'{newReplaceWord}'** by **'{user.Username}'**", "ReplaceWord successfully changed!"));
                WriteToConsol($"Information: | Task: BadWordChangeReplaceWord | Guild: {guildID} | BadWord: {badword} | User: {user} | /badwordchangereplaceword successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "ReplaceWord could not be changed!", "Error!"));
                WriteToConsol($"Error: | Task: BadWordChangeReplaceWord | Guild: {guildID} | BadWord: {badword} | User: {user} | Failed to change ReplaceWord | {ex.Message}");
                return;
            }
        }

        private static async Task BadWordRemove(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user)
        {
            var badword = GetOptions(parsedArg.Data.Options)[0].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            badword = badword.Replace("'", "’");

            //Check for valid input
            if (CheckNameLength(interaction, "", guildID, badword, "BadWordRemove", 20, false) ||
                CheckIfBadWordExists(interaction, badword, guildID, "BadWordRemove"))
            {
                return;
            }

            try
            {
                badwords.RemoveBadWord(badword);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"BadWord **'{badword}'** successfully removed by **'{user.Username}'**", "BadWord successfully removed!"));
                WriteToConsol($"Information: | Task: BadWordRemove | Guild: {guildID} | BadWord: {badword} | User: {user} | /bedwordremove successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "BadWord could not be removed!", "Error!"));
                WriteToConsol($"Error: | Task: BadWordRemove | Guild: {guildID} | BadWord: {badword} | User: {user} | Failed to remove BadWord | {ex.Message}");
                return;
            }
        }

        private static async Task BadWordAdd(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user)
        {
            var badword = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var replaceWord = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            badword = badword.Replace("'", "’");
            replaceWord = replaceWord.Replace("'", "’");

            //Check for valid input
            if (CheckNameLength(interaction, "", guildID, replaceWord, "BadWordAdd", 20, false) ||
                CheckNameLength(interaction, "", guildID, badword, "BadWordAdd", 20, false) ||
                CheckIfBadWordDouble(interaction, badword, guildID, "BadWordAdd"))
            {
                return;
            }

            try
            {
                badwords.AddBadWord(guildID, badword, replaceWord);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"BadWord **'{badword}'** successfully added by **'{user.Username}'**", "BadWord successfully added!"));
                WriteToConsol($"Information: | Task: BadWordAdd | Guild: {guildID} | BadWord: {badword} | User: {user} | /bedwordadd successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "BadWord could not be added!", "Error!"));
                WriteToConsol($"Error: | Task: BadWordAdd | Guild: {guildID} | BadWord: {badword} | User: {user} | Failed to add BadWord | {ex.Message}");
                return;
            }
        }

        private static async Task ComRegister(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user, DiscordSocketClient client)
        {
            var regCommand = GetOptions(parsedArg.Data.Options)[0].Value.ToString();

            if (CheckIfItsBobSty(interaction, guildID, user, parsedArg, "ComRegister", true))
            {
                return;
            }

            switch (regCommand)
            {
                case "tempinfo":
                    await RegisterCommands.RegisterTempInfoCommand(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
                case "helpbobii":
                    await RegisterCommands.RegisterHelpCommand(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
                case "tempadd":
                    await RegisterCommands.RegisterTempAddCommand(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
                case "tempremove":
                    await RegisterCommands.RegisterTempRemoveCommand(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
                case "tempchangename":
                    await RegisterCommands.RegisterTempChangeName(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
                case "comdelete":
                    await RegisterCommands.RegisterComRemoveCommand(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
                case "comdeleteguild":
                    await RegisterCommands.RegisterComRemoveGuildCommand(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
                case "comregister":
                    await RegisterCommands.RegisterComRegisterCommand(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
                case "badwordadd":
                    try
                    {
                        await RegisterCommands.RegisterBadWordAddCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    }
                    catch (Exception ex)
                    {
                        CommandRegisteredErrorRespond(interaction, guildID, regCommand, user, ex.Message);
                    }
                    break;
                case "badwordremove":
                    try
                    {
                        await RegisterCommands.RegisterBadWordRemoveCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    }
                    catch (Exception ex)
                    {
                        CommandRegisteredErrorRespond(interaction, guildID, regCommand, user, ex.Message);
                    }
                    break;
                case "badwordchangereplaceword":
                    try
                    {
                        await RegisterCommands.RegisterBadWordUpdateCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    }
                    catch (Exception ex)
                    {
                        CommandRegisteredErrorRespond(interaction, guildID, regCommand, user, ex.Message);
                    }
                    break;
                case "badwordinfo":
                    await RegisterCommands.RegisterBadWordInfoCommand(client);
                    CommandRegisteredRespond(interaction, guildID, regCommand, user);
                    break;
            }
        }


        private static async Task ComDeleteGuildSlashCommands(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user, DiscordSocketClient client)
        {
            var delCommand = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var delGuildID = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            if (CheckIfItsBobSty(interaction, guildID, user, parsedArg, "ComDeleteGuild", true) ||
                CheckDiscordID(interaction, delGuildID, guildID, "ComDeleteGuild", false))
            {
                return;
            }

            var commands = client.Rest.GetGuildApplicationCommands(ulong.Parse(delGuildID));

            foreach (Discord.Rest.RestGuildCommand command in commands.Result)
            {
                if (command.Name == delCommand)
                {
                    try
                    {
                        await command.DeleteAsync();
                        await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The command **'/{command.Name}'** was sucessfully deleted by the one and only **{user.Username}**", "Command successfully deleted"));
                        WriteToConsol($"Information: | Task: ComDeleteGuild | GuildWithCommand: {delGuildID} | Command: /{command.Name} | User: {user} | /comdeleteguild successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command **'/{command.Name}'** could not be removed", "Error!"));
                        WriteToConsol($"Error: | Task: ComDeleteGuild | Guild: {guildID} | Command: /{command.Name} | User: {user} | Failed to delete | {ex.Message}");
                        return;
                    }
                }
            }

            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command {delCommand} could not be found!", "Error!"));
            WriteToConsol($"Error: | Task: ComDeleteGuild | Guild: {guildID} | Command: /{delCommand} | User: {user} | No command with this name found");
            return;
        }

        private static async Task ComDeleteGlobalSlashCommands(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user, DiscordSocketClient client)
        {
            var delCommand = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var commands = client.Rest.GetGlobalApplicationCommands();

            if (CheckIfItsBobSty(interaction, guildID, user, parsedArg, "ComDelete", true))
            {
                return;
            }

            foreach (Discord.Rest.RestGlobalCommand command in commands.Result)
            {
                if (command.Name == delCommand)
                {
                    try
                    {
                        await command.DeleteAsync();
                        await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The command **'/{command.Name}'** was sucessfully deleted by the one and only **{user.Username}**", "Command successfully deleted"));
                        WriteToConsol($"Information: | Task: ComDelete | Guild: {guildID} | Command: /{command.Name} | User: {user} | /comdelete successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command **'/{command.Name}'** could not be removed", "Error!"));
                        WriteToConsol($"Error: | Task: ComDelete | Guild: {guildID} | Command: /{command.Name} | User: {user} | Failed to delete | {ex.Message}");
                        return;
                    }
                }
            }

            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command {delCommand} could not be found!", "Error!"));
            WriteToConsol($"Error: | Task: ComDelete | Guild: {guildID} | Command: /{delCommand} | User: {user} | No command with this name found");
            return;
        }

        private static async Task TempAdd(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuild guild, SocketGuildUser user)
        {
            var createChannelID = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var name = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "TempAdd") ||
                CheckDiscordID(interaction, createChannelID, guildID, "TempAdd", true) ||
                CheckDoubleCreateTempChannel(interaction, createChannelID, guildID, "TempAdd") ||
                CheckNameLength(interaction, createChannelID, guildID, name, "TempAdd", 50, true))
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            name = name.Replace("'", "’");

            try
            {
                createtempchannels.AddCC(guildID, name, createChannelID);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel **'{guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully added by **{user.Username}**", "CreateTempChannel sucessfully added!"));
                WriteToConsol($"Information: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tempadd successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "CreateTempChannel could not be added", "Error!"));
                WriteToConsol($"Error: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | Failed to add CreateTempChannel | {ex.Message}");
                return;
            }
        }

        private static async Task TempRemove(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuild guild, SocketGuildUser user)
        {
            var createChannelID = GetOptions(parsedArg.Data.Options)[0].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "TempRemove") ||
                CheckDiscordID(interaction, createChannelID, guildID, "TempRemove", true) ||
                CheckIfCreateTempChannelExists(interaction, createChannelID, guildID, "TempRemove"))
            {
                return;
            }

            try
            {
                createtempchannels.RemoveCC(guildID, createChannelID);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel **'{guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully removed by **{user.Username}**", "Successfully removed!"));
                WriteToConsol($"Information: | Task: TempRemove | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tempremove successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "CreateTempChannel could not be removed", "Error!"));
                WriteToConsol($"Error: | Task: TempRemove | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | Failed to remove CreateTempChannel | {ex.Message}");
                return;
            }
        }

        private static async Task TempChangeName(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user)
        {
            var createChannelID = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var voiceNameNew = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "TempChangeName") ||
                CheckDiscordID(interaction, createChannelID, guildID, "TempChangeName", true) ||
                CheckIfCreateTempChannelExists(interaction, createChannelID, guildID, "TempChangeName") ||
                CheckNameLength(interaction, createChannelID, guildID, voiceNameNew, "TempChangeName", 50, true))
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            voiceNameNew = voiceNameNew.Replace("'", "’");

            try
            {
                createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"TempChannelName succesfully changed to: **'{voiceNameNew}'**", "Name successfully changed!"));
                WriteToConsol($"Information: | Task: TempChangeName | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tempchangename successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "TempChannelName could not be changed", "Error!"));
                WriteToConsol($"Error: | Task: TempChangeName | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | Failed to change TempChannelName | {ex.Message}");
                return;
            }
            createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
            await Task.CompletedTask;
        }
        #endregion
    }
}
