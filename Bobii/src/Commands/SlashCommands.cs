using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bobii.src.DBStuff.Tables;
using Discord.Rest;
using Discord;

namespace Bobii.src.Commands
{
    class SlashCommands
    {
        #region Handler  
        public static async Task ModuleHandler(SocketInteraction interaction)
        {

        }

        public static async Task SlashCommandHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parsedArg = (SocketSlashCommand)interaction;
            var user = (SocketGuildUser)parsedArg.User;
            var guildID = TextChannel.TextChannel.GetGuildWithInteraction(interaction).Id.ToString();
            var guild = TextChannel.TextChannel.GetGuildWithInteraction(interaction);

            switch (parsedArg.Data.Name)
            {
                case "tcinfo":
                    await interaction.RespondAsync("", false, TempVoiceChannel.TempVoiceChannel.CreateVoiceChatInfoEmbed(guildID, client, interaction));
                    WriteToConsol($"Information: | Task: TempInfo | Guild: {guildID} | /tcinfo successfully used");
                    break;
                case "helpbobii":
                    await BobiiHelp(parsedArg, interaction, guildID, user, client);
                    WriteToConsol($"Information: | Task: Help | Guild: {guildID} | /helpbobii successfully used");
                    break;
                case "tcadd":
                    await TempAdd(parsedArg, interaction, guildID, guild, user);
                    break;
                case "tcremove":
                    await TempRemove(parsedArg, interaction, guildID, guild, user);
                    break;
                case "tcupdate":
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
                case "fwadd":
                    await FilterWordAdd(parsedArg, interaction, guildID, user);
                    break;
                case "fwremove":
                    await FilterWordRemove(parsedArg, interaction, guildID, user);
                    break;
                case "fwupdate":
                    await FilterWordUpdate(parsedArg, interaction, guildID, user);
                    break;
                case "fwinfo":
                    await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateFilterWordEmbed(interaction, guildID));
                    WriteToConsol($"Information: | Task: FilterWordInfo | Guild: {guildID} | /fwinfo successfully used");
                    break;
                case "testhelp":
                    break;
            }
        }
        #endregion

        #region Functions
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
            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The command **'/{commandName}'** failed to register", "Command failed to register"), ephemeral: true);
            WriteToConsol($"Error: | Task: ComRegister | Guild: {guildID} | Command: /{commandName} | Failed to register | {exMessage}");
        }
        #endregion

        #region CheckData
        private static bool CheckIfVoiceID(SocketInteraction interaction, string Id, string guildID, string task, SocketGuild guild)
        {
            foreach (var channel in guild.VoiceChannels)
            {
                if(channel.Id.ToString() == Id)
                {
                    return false;
                }
            }
            interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The given channel ID **'{Id}'** does not belong to a voice channel!\nMake sure to copy the voice Channel ID directly from the voice Channel!", "Invalid ID!"), ephemeral: true);
            WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {Id} | Invalid ID");
            return true;
        }

        private static bool CheckDiscordID(SocketInteraction interaction, string Id, string guildID, string task, bool channel)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(Id, out _) || Id.Length != 18)
            {
                if (channel)
                {
                    interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The given channel ID **'{Id}'** is not valid!\nMake sure to copy the ID from the voice channel directly!", "Invalid ID!"), ephemeral: true);
                    WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {Id} | Invalid ID");
                    return true;
                }
                else
                {
                    interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The given guild ID **'{Id}'** is not valid!\nMake sure to copy the ID from the guild directly!", "Invalid ID!"), ephemeral: true);
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
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The create-temp-channel with the ID **'{createChannelID}'** already exists!\nYou can get a list of all create-temp-channels by using:\n**/tcinfo**", "Create-temp-channel exists already!"), ephemeral: true);
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | Double CreateTempChannel");
                return true;
            }
            return false;
        }

        private static bool CheckIfCreateTempChannelExists(SocketInteraction interaction, string createChannelID, string guildID, string task)
        {
            if (!createtempchannels.CheckIfCreateVoiceChannelExist(guildID, createChannelID))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The create-temp-channel with the ID **'{createChannelID}'** does not exists!\nYou can get a list of all create-temp-channels by using:\n**/tcinfo**", "Create-temp-channel does not exist!"), ephemeral: true);
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | CreateTempChannel does not exist");
                return true;
            }
            return false;
        }

        private static bool CheckIfFilterWordDouble(SocketInteraction interaction, string filterWord, string guildID, string task)
        {
            if (filterwords.CheckIfFilterExists(guildID, filterWord))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The filter word with the name **'{filterWord}'** already exists!\nYou can get a list of all filter words by using:\n**/fwinfo**", "Filter word already exists!"), ephemeral: true);
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | Filter word: {filterWord} | Double filter word");
                return true;
            }
            return false;
        }

        private static bool CheckIfFilterWordExists(SocketInteraction interaction, string filterWord, string guildID, string task)
        {
            if (!filterwords.CheckIfFilterExists(guildID, filterWord))
            {
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The Filter word with the name **'{filterWord}'** does not exists!\nYou can get a list of all filter words by using:\n**/fwinfo**", "Filter word does not exist!"), ephemeral: true);
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | Filter word: {filterWord} | Filter word does not exist");
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
                    interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The TempChannelName **'{name}'** has more than 50 characters, pls make sure the name is shorter than {lenght} characters!", "Too much characters!"), ephemeral: true);
                    WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | CreateChannelID: {createChannelID} | TempChannelName: {name} | Name has too much characters");
                    return true;
                }
                else
                {
                    interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The the word **'{name}'** has more than {lenght} characters, pls make sure the name is shorter than {lenght} characters!", "Too much characters!"), ephemeral: true);
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
            interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Data.Name}**\nYou need one fo the following permissions to use this command:\n**Administrator**\n**Manage Server**", "Missing permissions!"), ephemeral: true);
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
                interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Data.Name}**\n**__Only BobSty himselfe is allowed to use this command!__**", "Missing permissions!"), ephemeral: true);
                WriteToConsol($"Error: | Task: {task} | Guild: {guildID} | User: {user} | Tryed to delete command: {GetOptions(parsedArg.Data.Options)[0].Value} | Someone tryed to be Me");
            }
            return true;
        }
        #endregion

        #region Tasks 
        private static async Task BobiiHelp(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser use, DiscordSocketClient client)
        {
            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "I have a lot of commands, so I have divided my commands into sections.\nYou can select the section from which you want to know the commands in the selection box.", "Bobii help:"), component: new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                    .WithCustomId("help-selector")
                    .WithPlaceholder("Select the section here!")
                    .WithOptions(new List<SelectMenuOptionBuilder>
                    {
                new SelectMenuOptionBuilder()
                    .WithLabel("Temporary Voice Channel")
                    .WithValue("temp-channel-help-selectmenuoption")
                    .WithDescription("All my commands to manage temp channels"),

                new SelectMenuOptionBuilder()
                    .WithLabel("Filter Word")
                    .WithValue("filter-word-help-selectmenuoption")
                    .WithDescription("All my commands to manage filter words")
                    }))
                .Build());
        }

        // Not used in the moment but still in here so I have an example
        private static async Task DelayAndDeletMessage(SocketInteraction interaction, int timeInSeconds)
        {
            timeInSeconds = timeInSeconds * 1000;
            var originalAsync = interaction.GetOriginalResponseAsync();
            await Task.Delay(timeInSeconds);
            await originalAsync.Result.DeleteAsync();
        }

        private static async Task FilterWordUpdate(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user)
        {
            var filterWord = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var newReplaceWord = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");
            newReplaceWord = newReplaceWord.Replace("'", "’");

            //Check for valid input + permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "FilterWordRemove") ||
                CheckNameLength(interaction, "", guildID, newReplaceWord, "FilterWordUpdate", 20, false) ||
                CheckNameLength(interaction, "", guildID, filterWord, "FilterWordUpdate", 20, false) ||
                CheckIfFilterWordExists(interaction, filterWord, guildID, "FilterWordUpdate"))
            {
                return;
            }
            try
            {
                filterwords.UpdateFilterWord(filterWord, newReplaceWord);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The filter word **'{filterWord}'** will now be replaced with **'{newReplaceWord}'**", "Successfully updated!"));
                WriteToConsol($"Information: | Task: FilterWordUpdate | Guild: {guildID} | Filter word: {filterWord} | User: {user} | /fwupdate successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The word which should replace **'{filterWord}'** could not be changed!", "Error!"), ephemeral: true);
                WriteToConsol($"Error: | Task: FilterWordUpdate | Guild: {guildID} | Filter word: {filterWord} | User: {user} | Failed to update the replace word | {ex.Message}");
                return;
            }
        }

        private static async Task FilterWordRemove(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user)
        {
            var filterWord = GetOptions(parsedArg.Data.Options)[0].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");

            //Check for valid input + permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "FilterWordRemove") ||
                CheckNameLength(interaction, "", guildID, filterWord, "FilterWordRemove", 20, false) ||
                CheckIfFilterWordExists(interaction, filterWord, guildID, "FilterWordRemove"))
            {
                return;
            }

            try
            {
                filterwords.RemoveFilterWord(filterWord);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The filter word **'{filterWord}'** was successfully removed by **'{user.Username}'**", "Filter word successfully removed!"));
                WriteToConsol($"Information: | Task: FilterWordRemove | Guild: {guildID} | Filter word: {filterWord} | User: {user} | /fwremove successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "The filter word could not be removed!", "Error!"), ephemeral: true);
                WriteToConsol($"Error: | Task: FilterWordRemove | Guild: {guildID} | Filter word: {filterWord} | User: {user} | Failed to remove filter word | {ex.Message}");
                return;
            }
        }

        private static async Task FilterWordAdd(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuildUser user)
        {
            var filterWord = GetOptions(parsedArg.Data.Options)[0].Value.ToString();
            var replaceWord = GetOptions(parsedArg.Data.Options)[1].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");
            replaceWord = replaceWord.Replace("'", "’");

            //Check for valid input and permission
            if (CheckUserPermission(interaction, guildID, user, parsedArg, "FilterWordAdd") ||
                CheckNameLength(interaction, "", guildID, replaceWord, "FilterWordAdd", 20, false) ||
                CheckNameLength(interaction, "", guildID, filterWord, "FilterWordAdd", 20, false) ||
                CheckIfFilterWordDouble(interaction, filterWord, guildID, "FilterWordAdd"))
            {
                return;
            }

            try
            {
                filterwords.AddFilterWord(guildID, filterWord, replaceWord);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The filter word **'{filterWord}'** was successfully added by **'{user.Username}'**", "Filter word successfully added!"));
                WriteToConsol($"Information: | Task: FilterWordAdd | Guild: {guildID} | Filter word: {filterWord} | User: {user} | /fwadd successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "The filter word could not be added!", "Error!"), ephemeral: true);
                WriteToConsol($"Error: | Task: FilterWordAdd | Guild: {guildID} | Filter word: {filterWord} | User: {user} | Failed to add filter word | {ex.Message}");
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

            try
            {
                switch (regCommand)
                {
                    case "tcinfo":
                        await RegisterCommands.RegisterTempInfoCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                    case "helpbobii":
                        await RegisterCommands.RegisterHelpCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                    case "tcadd":
                        await RegisterCommands.RegisterTempAddCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                    case "tcremove":
                        await RegisterCommands.RegisterTempRemoveCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                    case "tcupdate":
                        await RegisterCommands.RegisterTempUpdate(client);
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
                    case "fwadd":
                        await RegisterCommands.RegisterFilterWordAddCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                    case "fwremove":
                        await RegisterCommands.RegisterFilterWordRemoveCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                    case "fwupdate":
                        await RegisterCommands.RegisterFilterWordUpdateCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                    case "fwinfo":
                        await RegisterCommands.RegisterFilterWordInfoCommand(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                    case "testhelp":
                        await RegisterCommands.RegisterTestHelp(client);
                        CommandRegisteredRespond(interaction, guildID, regCommand, user);
                        break;
                }
            }
            catch (Exception ex)
            {
                CommandRegisteredErrorRespond(interaction, guildID, regCommand, user, ex.Message);
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
                        await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command **'/{command.Name}'** could not be removed", "Error!"), ephemeral: true);
                        WriteToConsol($"Error: | Task: ComDeleteGuild | Guild: {guildID} | Command: /{command.Name} | User: {user} | Failed to delete | {ex.Message}");
                        return;
                    }
                }
            }

            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command {delCommand} could not be found!", "Error!"), ephemeral: true);
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
                        await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command **'/{command.Name}'** could not be removed", "Error!"), ephemeral: true);
                        WriteToConsol($"Error: | Task: ComDelete | Guild: {guildID} | Command: /{command.Name} | User: {user} | Failed to delete | {ex.Message}");
                        return;
                    }
                }
            }

            await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Command {delCommand} could not be found!", "Error!"), ephemeral: true);
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
                CheckIfVoiceID(interaction, createChannelID, guildID, "TempAdd", guild) ||
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
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The create-temp-channel **'{guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully added by **{user.Username}**", "Create-temp-channel sucessfully added!"));
                WriteToConsol($"Information: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tcadd successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "Create-temp-channel could not be added", "Error!"), ephemeral: true);
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
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The create-temp-channel **'{guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully removed by **{user.Username}**", "Create-temp-channel successfully removed!"));
                WriteToConsol($"Information: | Task: TempRemove | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tcremove successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "Create-temp-channel could not be removed", "Error!"), ephemeral: true);
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
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"Temp-channel name succesfully changed to: **'{voiceNameNew}'**", "Name successfully changed!"));
                WriteToConsol($"Information: | Task: TempChangeName | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | /tcupdate successfully used");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "Temp-channel name could not be changed", "Error!"), ephemeral: true);
                WriteToConsol($"Error: | Task: TempChangeName | Guild: {guildID} | CreateChannelID: {createChannelID} | User: {user} | Failed to update TempChannelName | {ex.Message}");
                return;
            }
            createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
            await Task.CompletedTask;
        }
        #endregion
    }
}
