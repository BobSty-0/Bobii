using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bobii.src.DBStuff.Tables;
using Discord.Rest;
using Discord;
using Bobii.src.Entities;

namespace Bobii.src.Commands
{
    class SlashCommands
    {
        #region Handler  
        public static async Task SlashCommandHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var slashCommand = (SocketSlashCommand)interaction;
            var user = (SocketGuildUser)slashCommand.User;
            var guildID = TextChannel.TextChannel.GetGuildWithInteraction(interaction).Id.ToString();
            var guild = TextChannel.TextChannel.GetGuildWithInteraction(interaction);

            var parameter = new SlashCommandParameter();
            parameter.SlashCommand = (SocketSlashCommand)interaction;
            parameter.GuildUser = (SocketGuildUser)parameter.SlashCommand.User;
            parameter.Guild = TextChannel.TextChannel.GetGuildWithInteraction(interaction);
            parameter.GuildID = TextChannel.TextChannel.GetGuildWithInteraction(interaction).Id;
            parameter.Interaction = interaction;
            parameter.Client = client;

            switch (slashCommand.Data.Name)
            {
                case "tcinfo":
                    await TCInfo(parameter);
                    break;
                case "bobiiguides":
                    await BobiiGuides(parameter);
                    break;
                case "helpbobii":
                    await BobiiHelp(parameter);
                    break;
                case "tcadd":
                    await TempAdd(parameter);
                    break;
                case "tcremove":
                    await TempRemove(parameter);
                    break;
                case "tcupdate":
                    await TempChangeName(parameter);
                    break;
                case "comdelete":
                    await ComDeleteGlobalSlashCommands(parameter);
                    break;
                case "comdeleteguild":
                    await ComDeleteGuildSlashCommands(parameter);
                    break;
                case "comregister":
                    await ComRegister(parameter);
                    break;
                case "fwadd":
                    await FilterWordAdd(parameter);
                    break;
                case "fwremove":
                    await FilterWordRemove(parameter);
                    break;
                case "fwupdate":
                    await FilterWordUpdate(parameter);
                    break;
                case "fwinfo":
                    await FWInfo(parameter);
                    break;
                case "flinfo":
                    await FilterLinkInfo(parameter);
                    break;
                case "flset":
                    await FilterLinkSet(parameter);
                    break;
                case "flladd":
                    await FilterLinkWhitelistAdd(parameter);
                    break;
                case "fllremove":
                    await FilterLinkWhitelistRemove(parameter);
                    break;
                case "fluadd":
                    await FilterLinkWhitelistUserAdd(parameter);
                    break;
                case "fluremove":
                    await FilterLinkWhitelistUserRemove(parameter);
                    break;
                case "logset":
                    await FilterLinkLogSet(parameter);
                    break;
                case "logupdate":
                    await FilterLinkLogUpdate(parameter);
                    break;
                case "logremove":
                    await FilterLinkLogRemove(parameter);
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
        public static async void WriteToConsol(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} SCommands   {message}", color);
            await Task.CompletedTask;
        }
        #endregion

        #region CheckData
        private static bool CheckIfVoiceID(SocketInteraction interaction, string Id, string task, SocketGuild guild)
        {
            foreach (var channel in guild.VoiceChannels)
            {
                if (channel.Id.ToString() == Id)
                {
                    return false;
                }
            }
            interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The given channel ID **'{Id}'** does not belong to a voice channel!\nMake sure to copy the voice Channel ID directly from the voice Channel!", "Invalid ID!") }, ephemeral: true);
            WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {Id} | Invalid ID");
            return true;
        }

        private static bool CheckUserID(SocketInteraction interaction, string userId, SocketGuild guild, string task)
        {
            if (!ulong.TryParse(userId, out _) || userId.Length != 18)
            {

                interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The given channel ID **'{userId}'** is not valid!\nMake sure to copy the ID from the **user** directly!", "Invalid ID!") }, ephemeral: true);
                WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | UserID: {userId} | Invalid ID");
                return true;
            }
            return false;
        }

        private static bool CheckDiscordChannelID(SocketInteraction interaction, string Id, SocketGuild guild, string task, bool channel)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(Id, out _) || Id.Length != 18)
            {
                if (channel)
                {
                    interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The given channel ID **'{Id}'** is not valid!\nMake sure to copy the ID from the **voice channel** directly!", "Invalid ID!") }, ephemeral: true);
                    WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {Id} | Invalid ID");
                    return true;
                }
                else
                {
                    interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The given guild ID **'{Id}'** is not valid!\nMake sure to copy the ID from the guild directly!", "Invalid ID!") }, ephemeral: true);
                    WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | GuildID: {Id} | Invalid ID");
                    return true;
                }

            }
            return false;
        }

        private static bool CheckDoubleCreateTempChannel(SocketInteraction interaction, string createChannelID, SocketGuild guild, string task)
        {
            if (createtempchannels.CheckIfCreateVoiceChannelExist(guild, createChannelID))
            {
                interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The create-temp-channel with the ID **'{createChannelID}'** already exists!\nYou can get a list of all create-temp-channels by using:\n**/tcinfo**", "Create-temp-channel exists already!") }, ephemeral: true);
                WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {createChannelID} | Double CreateTempChannel");
                return true;
            }
            return false;
        }

        private static bool CheckIfCreateTempChannelExists(SocketInteraction interaction, string createChannelID, SocketGuild guild, string task)
        {
            if (!createtempchannels.CheckIfCreateVoiceChannelExist(guild, createChannelID))
            {
                interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The create-temp-channel with the ID **'{createChannelID}'** does not exists!\nYou can get a list of all create-temp-channels by using:\n**/tcinfo**", "Create-temp-channel does not exist!") }, ephemeral: true);
                WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {createChannelID} | CreateTempChannel does not exist");
                return true;
            }
            return false;
        }

        private static bool CheckIfFilterWordDouble(SocketInteraction interaction, string filterWord, SocketGuild guild, string task)
        {
            if (filterwords.CheckIfFilterExists(guild.Id.ToString(), filterWord))
            {
                interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The filter word with the name **'{filterWord}'** already exists!\nYou can get a list of all filter words by using:\n**/fwinfo**", "Filter word already exists!") }, ephemeral: true);
                WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | Filter word: {filterWord} | Double filter word");
                return true;
            }
            return false;
        }

        private static bool CheckIfFilterWordExists(SocketInteraction interaction, string filterWord, SocketGuild guild, string task)
        {
            if (!filterwords.CheckIfFilterExists(guild.Id.ToString(), filterWord))
            {
                interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The Filter word with the name **'{filterWord}'** does not exists!\nYou can get a list of all filter words by using:\n**/fwinfo**", "Filter word does not exist!") }, ephemeral: true);
                WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | Filter word: {filterWord} | Filter word does not exist");
                return true;
            }
            return false;
        }

        private static bool CheckNameLength(SocketInteraction interaction, string createChannelID, SocketGuild guild, string name, string task, int lenght, bool tempchannel)
        {
            if (name.Length > lenght)
            {
                if (tempchannel)
                {
                    interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The TempChannelName **'{name}'** has more than 50 characters, pls make sure the name is shorter than {lenght} characters!", "Too much characters!") }, ephemeral: true);
                    WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {createChannelID} | TempChannelName: {name} | Name has too much characters");
                    return true;
                }
                else
                {
                    interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"The the word **'{name}'** has more than {lenght} characters, pls make sure the name is shorter than {lenght} characters!", "Too much characters!") }, ephemeral: true);
                    WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | Name has too much characters");
                    return true;
                }
            }
            return false;
        }

        private static bool CheckUserPermission(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, SocketSlashCommand parsedArg, string task)
        {
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild)
            {
                return false;
            }
            interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Data.Name}**\nYou need one fo the following permissions to use this command:\n**Administrator**\n**Manage Server**", "Missing permissions!") }, ephemeral: true);
            WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | User: {user}| Missing premissions");
            return true;
        }

        public static bool CheckIfItsBobSty(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, SocketSlashCommand parsedArg, string task, bool errorMessage)
        {
            //False = Its me
            //True = Its not me
            if (user.Id.ToString() == (410312323409117185).ToString())
            {
                return false;
            }
            if (errorMessage)
            {
                interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Data.Name}**\n**__Only BobSty himselfe is allowed to use this command!__**", "Missing permissions!") }, ephemeral: true);
                WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | User: {user} | Tryed to delete command: {GetOptions(parsedArg.Data.Options)[0].Value} | Someone tryed to be Me");
            }
            return true;
        }
        #endregion

        #region Tasks 
        private static async Task FilterLinkLogRemove(SlashCommandParameter parameter)
        {
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkLogRemove"))
            {
                return;
            }

            if (!filterlinklogs.DoesALogChannelExist(parameter.GuildID))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have a log channel yet, you can set a log channel by using:\n`/logset`", "No log channel yet!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| No filterlink log channel to update");
                return;
            }

            try
            {
                filterlinklogs.RemoveFilterLinkLog(parameter.GuildID);

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel was successfully removed", "Successfully removed") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | /logremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel could not be removed", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to remove log channel | {ex.Message}");
                return;
            }
        }

        private static async Task FilterLinkLogUpdate(SlashCommandParameter parameter)
        {
            var channelId = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            if (!channelId.StartsWith("<#"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Make sure to use #channel-name for the parameter <channel>\nYou can do that by simply typing # followed by the channel name", "Wrong input!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong channel input");
                return;
            }
            channelId = channelId.Replace("<", "");
            channelId = channelId.Replace(">", "");
            channelId = channelId.Replace("#", "");

            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkLogUpdate") ||
                CheckDiscordChannelID(parameter.Interaction, channelId, parameter.Guild, "FilterLinkLogUpdate", true))
            {
                return;
            }

            if (!filterlinklogs.DoesALogChannelExist(parameter.GuildID))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have a log channel yet, you can set a log channel by using:\n`/logset`", "No log channel yet!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| No filterlink log channel to update");
                return;
            }

            try
            {
                filterlinklogs.UpdateFilterLinkLog(parameter.GuildID, ulong.Parse(channelId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel was sucessfully changed to <#{channelId}>", "Successfully updated") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | /logupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel could not be updated", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | Failed to update log channel | {ex.Message}");
                return;
            }
        }

        private static async Task FilterLinkLogSet(SlashCommandParameter parameter)
        {
            var channelId = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            if (!channelId.StartsWith("<#"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Make sure to use #channel-name for the parameter <channel>\nYou can do that by simply typing # followed by the channel name", "Wrong input!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong channel input");
                return;
            }
            channelId = channelId.Replace("<", "");
            channelId = channelId.Replace(">", "");
            channelId = channelId.Replace("#", "");

            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkLogSet") ||
                CheckDiscordChannelID(parameter.Interaction, channelId, parameter.Guild, "FilterLinkLogSet", true))
            {
                return;
            }

            if (filterlinklogs.DoesALogChannelExist(parameter.GuildID))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You already set a log channel: <#{filterlinklogs.GetFilterLinkLogChannelID(parameter.GuildID)}>", "Already set!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLinkLog already set");
                return;
            }

            try
            {
                filterlinklogs.SetFilterLinkLog(parameter.GuildID, ulong.Parse(channelId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The channel <#{channelId}> will now show all messages which will be deleted by Bobii", "Log successfully set") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | /logset successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel could not be set", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | Failed to set log channel | {ex.Message}");
                return;
            }
        }

        private static async Task FWInfo(SlashCommandParameter parameter)
        {
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "fwinfo"))
            {
                return;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateFilterWordEmbed(parameter.Interaction, parameter.GuildID.ToString()) });
            WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterWordInfo | Guild: {parameter.GuildID} | /fwinfo successfully used");
        }

        private static async Task TCInfo(SlashCommandParameter parameter)
        {
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "tcinfo"))
            {
                return;
            }
            await parameter.Interaction.RespondAsync("", new Embed[] { TempVoiceChannel.TempVoiceChannel.CreateVoiceChatInfoEmbed(parameter.Guild, parameter.Client, parameter.Interaction) });
            WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempInfo | Guild: {parameter.GuildID} | /tcinfo successfully used");
        }

        private static async Task FilterLinkWhitelistUserRemove(SlashCommandParameter parameter)
        {
            //TODO Check for valid Id (also if user is on this server) -> replace the old functions
            var userId = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            if (!userId.StartsWith("<@") && !userId.EndsWith(">"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Make shure to use @User for the parameter <user>", "Wrong input!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong User input");
                return;
            }
            userId = userId.Replace("<", "");
            userId = userId.Replace(">", "");
            userId = userId.Replace("@", "");
            userId = userId.Replace("!", "");

            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkWhitelistUserAdd") ||
                CheckUserID(parameter.Interaction, userId, parameter.Guild, "FilterLinkWhitelistUserAdd"))
            {
                return;
            }

            if (!filterlinkuserguild.IsUserOnWhitelistInGuild(parameter.GuildID, ulong.Parse(userId)))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is not on the whitelisted", "Not on whitelist!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already whitelisted");
                return;
            }

            try
            {
                filterlinkuserguild.RemoveWhiteListUserFromGuild(parameter.GuildID, ulong.Parse(userId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is no longer on the whitelist", "User successfully removed") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {userId} | /fluremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"User could not be added to the whitelist", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | User: {userId} | Failed to remove user from whitelist | {ex.Message}");
                return;
            }
        }

        private static async Task FilterLinkWhitelistUserAdd(SlashCommandParameter parameter)
        {
            //TODO Check for valid Id (also if user is on this server) -> replace the old functions
            var userId = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            if (!userId.StartsWith("<@") && !userId.EndsWith(">"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Make shure to use @User for the parameter <user>", "Wrong input!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong User input");
                return;
            }
            userId = userId.Replace("<", "");
            userId = userId.Replace(">", "");
            userId = userId.Replace("@", "");
            userId = userId.Replace("!", "");

            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkWhitelistUserAdd") ||
                CheckUserID(parameter.Interaction, userId, parameter.Guild, "FilterLinkWhitelistUserAdd"))
            {
                return;
            }

            if (filterlinkuserguild.IsUserOnWhitelistInGuild(parameter.GuildID, ulong.Parse(userId)))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is already whitelisted", "Already on whitelist!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already whitelisted");
                return;
            }

            try
            {
                var filterLinkActiveText = "";
                if (!filterlink.IsFilterLinkActive(parameter.GuildID))
                {
                    filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
                }

                filterlinkuserguild.AddWhiteListUserToGuild(parameter.GuildID, ulong.Parse(userId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The user  <@{userId}> is now on the whitelist.{filterLinkActiveText}", "User successfully added") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {userId} | /fluadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"User could not be added to the whitelist", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | User: {userId} | Failed to add user to whitelist | {ex.Message}");
                return;
            }
        }

        private static async Task FilterLinkInfo(SlashCommandParameter parameter)
        {
            //inks = 1 / user = 2
            var linkoruser = int.Parse(GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString());

            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkInfo"))
            {
                return;
            }

            if (linkoruser == 1)
            {
                await parameter.Interaction.RespondAsync("", new Embed[] { TextChannel.TextChannel.CreateFilterLinkLinkWhitelistInfoEmbed(parameter.Interaction, parameter.GuildID) });
            }
            else
            {
                await parameter.Interaction.RespondAsync("", new Embed[] { TextChannel.TextChannel.CreateFilterLinkUserWhitelistInfoEmbed(parameter.Interaction, parameter.GuildID) });
            }

        }

        private static async Task FilterLinkWhitelistRemove(SlashCommandParameter parameter)
        {
            var link = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkWhitelistRemove"))
            {
                return;
            }
            if (!filterlinksguild.IsFilterlinkAllowedInGuild(parameter.GuildID, link))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Links of **{link}** are not whitelisted yet", "Not on whitelist!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink is not on whitelist");
                return;
            }

            try
            {
                filterlinksguild.RemoveFromGuild(parameter.GuildID, link);

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"**{link}** links are no longer on the whitelist", "Link successfully removed") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | /flwremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Link could not be removed from the whitelist", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | Failed to remove link from the whitelist | {ex.Message}");
                return;
            }
        }

        private static async Task FilterLinkWhitelistAdd(SlashCommandParameter parameter)
        {
            var link = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkWhitelistAdd"))
            {
                return;
            }
            if (filterlinksguild.IsFilterlinkAllowedInGuild(parameter.GuildID, link))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Links of **{link}** are already whitelisted", "Already on whitelist!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already whitelisted");
                return;
            }

            try
            {
                var filterLinkActiveText = "";
                if (!filterlink.IsFilterLinkActive(parameter.GuildID))
                {
                    filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
                }
                filterlinksguild.AddToGuild(parameter.GuildID, link);

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"**{link}** links are now on the whitelist. {filterLinkActiveText}", "Link successfully added") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | /flwadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Link could not be added to the whitelist", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | Failed to add link to whitelist | {ex.Message}");
                return;
            }
        }

        private static async Task FilterLinkSet(SlashCommandParameter parameter)
        {
            var state = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            //Check for valid input + permission
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkSet"))
            {
                return;
            }

            if (state == "2")
            {
                if (!filterlink.IsFilterLinkActive(parameter.GuildID))
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Filter link is already inactive", "Already inactive!") }, ephemeral: true);
                    WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already inactive");
                    return;
                }
                try
                {
                    filterlink.DeactivateFilterLink(parameter.GuildID);

                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"I wont filter links anymore from now on!\nTo reactivate filter link use:\n`/flset`", "Filter link deactivated!") });
                    WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | State: active | /flset successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"State of filter link could not be set.", "Error!") }, ephemeral: true);
                    WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to set state | {ex.Message}");
                    return;
                }
            }
            else
            {
                if (filterlink.IsFilterLinkActive(parameter.GuildID))
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Filter link is already active", "Already active!") }, ephemeral: true);
                    WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already active");
                    return;
                }
                try
                {
                    filterlink.ActivateFilterLink(parameter.GuildID);

                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"I will from now on watch out for links!\nIf you want to whitelist specific links for excample YouTube links you can use:\n`/flladd`\nIf you want to add a user to the whitelist so that he can use links without restriction, then you can use:\n`/fluadd`", "Filter link activated!") });
                    WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | State: active | /flset successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"State of filter link could not be set.", "Error!") }, ephemeral: true);
                    WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to set state | {ex.Message}");
                    return;
                }
            }
        }

        private static async Task BobiiGuides(SlashCommandParameter parameter)
        {
            try
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "I'm planing on doing more guides in the future but for now there is only one to select in the select-menu below.\nYou can select the guid you wish to follow in the selection-menu.\nIf you are looking for commands, you can use the command: `/helpbobii`!", "Bobii help:") }, component: new ComponentBuilder()
                    .WithSelectMenu(new SelectMenuBuilder()
                        .WithCustomId("guide-selector")
                        .WithPlaceholder("Select the guide here!")
                        .WithOptions(new List<SelectMenuOptionBuilder>
                        {
                new SelectMenuOptionBuilder()
                    .WithLabel("Add create-temp-channel")
                    .WithValue("how-to-cereate-temp-channel-guide")
                    .WithDescription("Guid for /tcadd")
                        }))
                    .Build());
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: Guides | Guild {parameter.GuildID} | /bobiiguides successfully used");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static async Task BobiiHelp(SlashCommandParameter parameter)
        {
            await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "I have a lot of commands, so I have divided my commands into sections.\nYou can select the section from which you want to know the commands in the selection-menu below.\nIf you are looking for guides you can use the command: `/bobiiguides`!", "Bobii help:") }, component: new ComponentBuilder()
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
                    .WithDescription("All my commands to manage filter words"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Filter Link")
                    .WithValue("filter-link-help-selectmenuotion")
                    .WithDescription("All my commads to manage filter links"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Support")
                    .WithValue("support-help-selectmenuotion")
                    .WithDescription("Instruction on my support system")
                    }))
                .Build());
            WriteToConsol($"Information: {parameter.Guild.Name} | Task: Help | Guild: {parameter.GuildID} | /helpbobii successfully used");
        }

        private static async Task FilterWordUpdate(SlashCommandParameter parameter)
        {
            var filterWord = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            var newReplaceWord = GetOptions(parameter.SlashCommand.Data.Options)[1].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");
            newReplaceWord = newReplaceWord.Replace("'", "’");

            //Check for valid input + permission
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterWordRemove") ||
                CheckNameLength(parameter.Interaction, "", parameter.Guild, newReplaceWord, "FilterWordUpdate", 20, false) ||
                CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordUpdate", 20, false) ||
                CheckIfFilterWordExists(parameter.Interaction, filterWord, parameter.Guild, "FilterWordUpdate"))
            {
                return;
            }
            try
            {
                filterwords.UpdateFilterWord(filterWord, newReplaceWord, parameter.GuildID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** will now be replaced with **'{newReplaceWord}'**", "Successfully updated!") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterWordUpdate | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | /fwupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The word which should replace **'{filterWord}'** could not be changed!", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterWordUpdate | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | Failed to update the replace word | {ex.Message}");
                return;
            }
        }

        private static async Task FilterWordRemove(SlashCommandParameter parameter)
        {
            var filterWord = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");

            //Check for valid input + permission
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterWordRemove") ||
                CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordRemove", 20, false) ||
                CheckIfFilterWordExists(parameter.Interaction, filterWord, parameter.Guild, "FilterWordRemove"))
            {
                return;
            }

            try
            {
                filterwords.RemoveFilterWord(filterWord, parameter.GuildID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** was successfully removed by **'{parameter.GuildUser.Username}'**", "Filter word successfully removed!") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterWordRemove | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | /fwremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "The filter word could not be removed!", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterWordRemove | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | Failed to remove filter word | {ex.Message}");
                return;
            }
        }

        private static async Task FilterWordAdd(SlashCommandParameter parameter)
        {
            var filterWord = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            var replaceWord = GetOptions(parameter.SlashCommand.Data.Options)[1].Value.ToString();

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            filterWord = filterWord.Replace("'", "’");
            replaceWord = replaceWord.Replace("'", "’");

            //Check for valid input and permission
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterWordAdd") ||
                CheckNameLength(parameter.Interaction, "", parameter.Guild, replaceWord, "FilterWordAdd", 20, false) ||
                CheckNameLength(parameter.Interaction, "", parameter.Guild, filterWord, "FilterWordAdd", 20, false) ||
                CheckIfFilterWordDouble(parameter.Interaction, filterWord, parameter.Guild, "FilterWordAdd"))
            {
                return;
            }

            try
            {
                filterwords.AddFilterWord(parameter.GuildID, filterWord, replaceWord);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The filter word **'{filterWord}'** was successfully added by **'{parameter.GuildUser.Username}'**", "Filter word successfully added!") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterWordAdd | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | /fwadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "The filter word could not be added!", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterWordAdd | Guild: {parameter.GuildID} | Filter word: {filterWord} | User: {parameter.GuildUser} | Failed to add filter word | {ex.Message}");
                return;
            }
        }

        private static async Task ComRegister(SlashCommandParameter parameter)
        {
            var regCommand = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            if (CheckIfItsBobSty(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "ComRegister", true))
            {
                return;
            }

            await Handler.RegisterHandlingService.HandleRegisterCommands(parameter.Interaction, parameter.Guild, parameter.GuildUser, regCommand, parameter.Client);
        }

        //I did not test this after implementing the parameter thingi!!
        private static async Task ComDeleteGuildSlashCommands(SlashCommandParameter parameter)
        {
            var delCommand = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            var delGuildID = GetOptions(parameter.SlashCommand.Data.Options)[1].Value.ToString();

            if (CheckIfItsBobSty(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "ComDeleteGuild", true) ||
                CheckDiscordChannelID(parameter.Interaction, delGuildID, parameter.Guild, "ComDeleteGuild", false))
            {
                return;
            }

            var commands = parameter.Client.Rest.GetGuildApplicationCommands(ulong.Parse(delGuildID));

            foreach (Discord.Rest.RestGuildCommand command in commands.Result)
            {
                if (command.Name == delCommand)
                {
                    try
                    {
                        await command.DeleteAsync();
                        await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The command **'/{command.Name}'** was sucessfully deleted by the one and only **{parameter.GuildUser.Username}**", "Command successfully deleted") });
                        WriteToConsol($"Information: {parameter.Guild.Name} | Task: ComDeleteGuild | Guild: {parameter.GuildID} | GuildWithCommand: {delGuildID} | Command: /{command.Name} | User: {parameter.GuildUser} | /comdeleteguild successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Command **'/{command.Name}'** could not be removed", "Error!") }, ephemeral: true);
                        WriteToConsol($"Error: {parameter.Guild.Name} | Task: ComDeleteGuild | Guild: {parameter.GuildID} | Command: /{command.Name} | User: {parameter.GuildUser} | Failed to delete | {ex.Message}");
                        return;
                    }
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Command {delCommand} could not be found!", "Error!") }, ephemeral: true);
            WriteToConsol($"Error: {parameter.Guild.Name} | Task: ComDeleteGuild | Guild: {parameter.GuildID} | Command: /{delCommand} | User: {parameter.GuildUser} | No command with this name found");
            return;
        }


        //I did not Test this after changing the parameter thing!!
        private static async Task ComDeleteGlobalSlashCommands(SlashCommandParameter parameter)
        {
            var delCommand = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            var commands = parameter.Client.Rest.GetGlobalApplicationCommands();

            if (CheckIfItsBobSty(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "ComDelete", true))
            {
                return;
            }

            foreach (RestGlobalCommand command in commands.Result)
            {
                if (command.Name == delCommand)
                {
                    try
                    {
                        await command.DeleteAsync();
                        await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The command **'/{command.Name}'** was sucessfully deleted by the one and only **{parameter.GuildUser.Username}**", "Command successfully deleted") });
                        WriteToConsol($"Information: {parameter.Guild.Name} | Task: ComDelete | Guild: {parameter.GuildID} | Command: /{command.Name} | User: {parameter.GuildUser} | /comdelete successfully used");
                        return;
                    }
                    catch (Exception ex)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Command **'/{command.Name}'** could not be removed", "Error!") }, ephemeral: true);
                        WriteToConsol($"Error: {parameter.Guild.Name} | Task: ComDelete | Guild: {parameter.GuildID} | Command: /{command.Name} | User: {parameter.GuildUser} | Failed to delete | {ex.Message}");
                        return;
                    }
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Command {delCommand} could not be found!", "Error!") }, ephemeral: true);
            WriteToConsol($"Error: {parameter.Guild.Name} | Task: ComDelete | Guild: {parameter.GuildID} | Command: /{delCommand} | User: {parameter.GuildUser} | No command with this name found");
            return;
        }

        private static async Task TempAdd(SlashCommandParameter parameter)
        {
            var createChannelID = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            var name = GetOptions(parameter.SlashCommand.Data.Options)[1].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "TempAdd") ||
                CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempAdd", true) ||
                CheckIfVoiceID(parameter.Interaction, createChannelID, "TempAdd", parameter.Guild) ||
                CheckDoubleCreateTempChannel(parameter.Interaction, createChannelID, parameter.Guild, "TempAdd") ||
                CheckNameLength(parameter.Interaction, createChannelID, parameter.Guild, name, "TempAdd", 50, true))
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            name = name.Replace("'", "’");

            try
            {
                createtempchannels.AddCC(parameter.GuildID, name, createChannelID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The create-temp-channel **'{parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully added by **{parameter.GuildUser.Username}**", "Create-temp-channel sucessfully added!") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "Create-temp-channel could not be added", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to add CreateTempChannel | {ex.Message}");
                return;
            }
        }

        private static async Task TempRemove(SlashCommandParameter parameter)
        {
            var createChannelID = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "TempRemove") ||
                CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempRemove", true) ||
                CheckIfCreateTempChannelExists(parameter.Interaction, createChannelID, parameter.Guild, "TempRemove"))
            {
                return;
            }

            try
            {
                createtempchannels.RemoveCC(parameter.GuildID.ToString(), createChannelID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The create-temp-channel **'{parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully removed by **{parameter.GuildUser.Username}**", "Create-temp-channel successfully removed!") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempRemove | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "Create-temp-channel could not be removed", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempRemove | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to remove CreateTempChannel | {ex.Message}");
                return;
            }
        }

        private static async Task TempChangeName(SlashCommandParameter parameter)
        {
            var createChannelID = GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            var voiceNameNew = GetOptions(parameter.SlashCommand.Data.Options)[1].Value.ToString();

            //Checking for valid input and Permission
            if (CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "TempChangeName") ||
                CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempChangeName", true) ||
                CheckIfCreateTempChannelExists(parameter.Interaction, createChannelID, parameter.Guild, "TempChangeName") ||
                CheckNameLength(parameter.Interaction, createChannelID, parameter.Guild, voiceNameNew, "TempChangeName", 50, true))
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            voiceNameNew = voiceNameNew.Replace("'", "’");

            try
            {
                createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Temp-channel name succesfully changed to: **'{voiceNameNew}'**", "Name successfully changed!") });
                WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempChangeName | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "Temp-channel name could not be changed", "Error!") }, ephemeral: true);
                WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempChangeName | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to update TempChannelName | {ex.Message}");
                return;
            }
            createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
            await Task.CompletedTask;
        }
        #endregion
    }
}
