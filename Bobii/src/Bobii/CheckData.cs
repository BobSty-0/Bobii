using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    class CheckDatas
    {
        public static async Task<bool> CheckIfIDBelongsToVoiceChannel(SocketInteraction interaction, string Id, string task, SocketGuild guild, string language)
        {
            foreach (var channel in guild.VoiceChannels)
            {
                if (channel.Id.ToString() == Id)
                {
                    return false;
                }
            }

            await interaction.RespondAsync(null, new Embed[] {
                Helper.CreateEmbed(interaction,
                    String.Format(Helper.GetContent("C001", language).Result, Id),
                    Helper.GetCaption("C001", language).Result
                ).Result 
            }, ephemeral: true);

            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms",
                true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id }, 
                message: Helper.GetCaption("C001").Result);
            return true;
        }

        public static async Task<bool> CheckUserIDFormat(SocketInteraction interaction, string userId, SocketGuild guild, string task, string language)
        {
            if (!ulong.TryParse(userId, out _) || userId.Length != 18)
            {
                await interaction.RespondAsync(null, new Embed[] {
                    Helper.CreateEmbed(interaction,
                        String.Format(Helper.GetContent("C002", language).Result, userId),
                        Helper.GetCaption("C002", language).Result
                    ).Result 
                }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                    true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id },
                    iD: userId, message: Helper.GetCaption("C002").Result);
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckDiscordChannelIDFormat(SocketInteraction interaction, string Id, SocketGuild guild, string task, bool channel, string language)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(Id, out _) || Id.Length != 18)
            {
                if (channel)
                {
                    await interaction.RespondAsync(null, new Embed[] { 
                        Helper.CreateEmbed(interaction,
                            String.Format(Helper.GetContent("C003", language).Result, "channel", Id),
                            String.Format(Helper.GetCaption("C003", language).Result, "channel")
                        ).Result 
                    }, ephemeral: true);

                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                        true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id },
                        iD: Id, message: String.Format(Helper.GetCaption("C003").Result, "channel"));
                    return true;
                }
                else
                {
                    await interaction.RespondAsync(null, new Embed[] {
                        Helper.CreateEmbed(interaction,
                            String.Format(Helper.GetContent("C003", language).Result, "guild", Id),
                            String.Format(Helper.GetCaption("C003", language).Result, "guild")
                        ).Result 
                    }, ephemeral: true);

                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                        true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id },
                        iD: Id, message: String.Format(Helper.GetCaption("C003").Result, "guild"));
                    return true;
                }

            }
            return false;
        }

        public static async Task<bool> CheckIfCreateTempChannelWithGivenIDExists(SocketInteraction interaction, string createChannelID, 
            SocketGuild guild, string task, string language)
        {
            if (TempChannel.EntityFramework.CreateTempChannelsHelper.CheckIfCreateVoiceChannelExist(guild, ulong.Parse(createChannelID)).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { 
                    Helper.CreateEmbed(interaction,
                        String.Format(Helper.GetContent("C004", language).Result, createChannelID),
                        Helper.GetCaption("C004", language).Result
                    ).Result 
                }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                    true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id },
                    createChannelID: ulong.Parse(createChannelID), message: Helper.GetCaption("C004").Result);
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfChannelIDBelongsToACreateTempChannel(SocketInteraction interaction, 
            string createChannelID, SocketGuild guild, string task, string language)
        {
            if (TempChannel.EntityFramework.CreateTempChannelsHelper.CheckIfCreateVoiceChannelExist(guild, ulong.Parse(createChannelID)).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { 
                    Helper.CreateEmbed(interaction,
                        Helper.GetContent("C005", language).Result,
                        Helper.GetCaption("C005", language).Result
                    ).Result 
                }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                    true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id },
                    createChannelID: ulong.Parse(createChannelID), message: Helper.GetCaption("C005").Result);
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfCreateTempChannelWithGivenIDAlreadyExists(SocketInteraction interaction, 
            string createChannelID, SocketGuild guild, string task, string language)
        {
            if (!TempChannel.EntityFramework.CreateTempChannelsHelper.CheckIfCreateVoiceChannelExist(guild, ulong.Parse(createChannelID)).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { 
                    Helper.CreateEmbed(interaction,
                        String.Format(Helper.GetContent("C006", language).Result, createChannelID),
                        Helper.GetCaption("C006", language).Result)
                    .Result 
                }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                    true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id },
                    createChannelID: ulong.Parse(createChannelID), message: Helper.GetCaption("C006").Result);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Is Checking if the filterword does already exist
        /// </summary>
        public static async Task<bool> CheckIfFilterWordExists(SocketInteraction interaction, string filterWord, SocketGuild guild, string task, string language)
        {
            if (FilterWord.EntityFramework.FilterWordsHelper.CheckIfFilterWordExists(guild.Id, filterWord).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { 
                    Helper.CreateEmbed(interaction,
                        String.Format(Helper.GetContent("C007", language).Result, filterWord),
                        Helper.GetCaption("C007", language).Result).Result 
                }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                    true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id },
                    filterWord: filterWord, message: Helper.GetCaption("C007").Result);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Is Checking if the filterword does exist
        /// </summary>
        public static async Task<bool> CheckIfFilterWordAlreadyExists(SocketInteraction interaction, 
            string filterWord, SocketGuild guild, string task, string language)
        {
            if (!FilterWord.EntityFramework.FilterWordsHelper.CheckIfFilterWordExists(guild.Id, filterWord).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { 
                    Helper.CreateEmbed(interaction,
                        String.Format(Helper.GetContent("C008", language).Result, filterWord),
                        Helper.GetCaption( "C008", language).Result
                    ).Result 
                }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                    true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildID = guild.Id },
                    filterWord: filterWord, message: Helper.GetCaption("C008").Result);
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckNameLength(SocketInteraction interaction, string createChannelID, 
            SocketGuild guild, string name, string task, int lenght, bool tempchannel, string language)
        {
            if (name.Length > lenght)
            {
                if (tempchannel)
                {
                    await interaction.RespondAsync(null, new Embed[] { 
                        Helper.CreateEmbed(interaction,
                            String.Format(Helper.GetContent("C009", language).Result, "temp-channel-name", name, lenght),
                            String.Format( Helper.GetCaption("C009",language).Result, "temp-channel-name")
                        ).Result 
                    }, ephemeral: true);

                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", 
                        true, task, new Entities.SlashCommandParameter() { Guild = guild },
                        createChannelID: ulong.Parse(createChannelID), message: String.Format(Helper.GetCaption("C009").Result, "temp-channel-name"));
                    return true;
                }
                else
                {
                    await interaction.RespondAsync(null, new Embed[] { 
                        Helper.CreateEmbed(interaction,
                            String.Format(Helper.GetContent("C009", language).Result, "word", name, lenght),
                            String.Format(Helper.GetCaption("C009",language).Result, "word")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild },
                        message: String.Format(Helper.GetCaption("C009", language).Result, "word"));
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> CheckUserPermission(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, SocketSlashCommandData parsedArg, string task)
        {
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild)
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                $"You dont have the permissions to use:\n**/{parsedArg.Name}**\nYou need one fo the following permissions to use this command:\n**Administrator**\n**Manage Server**", "Missing permissions!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                message: "Missing premissions");
            return true;
        }

        public static async Task<bool> CheckMessageID(SocketInteraction interaction, SocketGuild guild, string id, string task, DiscordSocketClient client)
        {
            if (!ulong.TryParse(id, out _) || id.Length != 18)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"The given message ID **'{id}'** is not valid!\nMake sure to copy the ID from the **message wich includes the embed** directly!",
                    "Invalid ID!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild }, messageID: id,
                    message: "Invalid ID");
                return true;
            }

            var channel = (SocketTextChannel)client.GetChannel(interaction.Channel.Id);

            var messagesInChannel = channel.GetMessagesAsync(100).Flatten();
            if (messagesInChannel == null || messagesInChannel.ToArrayAsync().Result.Count() == 0)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"There are no messages in the channel which you just used the command in.\nUse this command in the chanenl with the messages which contains the embed you want to change!",
                    "No messages detected!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild }, messageID: id,
                    message: "Invalid ID");
                return true;
            }

            var message = messagesInChannel.ToArrayAsync().Result.Where(m => m.Id == ulong.Parse(id)).FirstOrDefault();
            if (message == null)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"There is no message in the channel in which you used this command with the given id **{id}**.\nPlease use this command in the channel which contains the message with the embed!",
                    "Invalid ID!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild }, messageID: id,
                    message: "Invalid ID");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfMessageFromCreateEmbed(SocketInteraction interaction, SocketGuild guild, ulong messageId, string task, DiscordSocketClient client)
        {
            var channel = (SocketTextChannel)client.GetChannel(interaction.Channel.Id);
            var messagesInChannel = channel.GetMessagesAsync(100).Flatten();
            var message = messagesInChannel.ToArrayAsync().Result.Where(m => m.Id == messageId).FirstOrDefault();
            // §TODO JG/220.11.2021 Check if this works
            if (!message.Author.IsBot || !(message.Author.Id == 776028262740393985 || message.Author.Id == 869180143363584060))
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"The given message id does not belong to a message from Bobii!", "Not from Bobii!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild }, messageID: messageId.ToString(),
                    message: "Message not from Bobii");
                return true;
            }

            if (message.Interaction != null)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"This message was not created with /tucreatetembed!", "Not created with create embed!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild }, messageID: messageId.ToString(),
                    message: "Message has an Interaction attached");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckStringLength(SocketInteraction interaction, SocketGuild guild, string stringToCheck, int maxLenth, string parameterName, string task)
        {
            if (stringToCheck.Length < maxLenth)
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The length of **{parameterName}** cannot be longer than {maxLenth}!", "Invalid length!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild }, parameterName: parameterName,
                message: "Invalid length of parameter");
            return true;
        }
        public static async Task<bool> CheckStringForAlphanumericCharacters(SocketInteraction interaction, SocketGuild guild, string stringToCheck, string task)
        {
            if (Regex.IsMatch(stringToCheck, @"^[a-zA-Z_ ]+$"))
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The Name can olny contain alphanumeric characters and `_`!", "Invalid characters!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild }, parameterName: stringToCheck,
                message: "Invalid character in Emoji name");
            return true;
        }

        public static async Task<bool> CheckMinLength(SocketInteraction interaction, SocketGuild guild, string stringToCheck, int minLength, string nameOfThingToTest, string task)
        {
            if (stringToCheck.Length > minLength)
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                $"The input of {nameOfThingToTest} has to have at least {minLength} characters!", "Not enough characters!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild }, parameterName: stringToCheck,
                message: "Not enough caracters");
            return true;
        }

        public static async Task<bool> CheckIfItsBobSty(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, SocketSlashCommandData parsedArg, string task, bool errorMessage)
        {
            //False = Its me
            //True = Its not me
            if (user.Id.ToString() == (410312323409117185).ToString())
            {
                return false;
            }
            if (errorMessage)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"You dont have the permissions to use:\n**/{parsedArg.Name}**\n**__Only BobSty himselfe is allowed to use this command!__**", "Missing permissions!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                    message: $"Tryed to delete command: {Handler.SlashCommandHandlingService.GetOptions(parsedArg.Options).Result[0].Value} | Someone tryed to be me");
            }
            return true;
        }

        public static async Task<bool> CheckIfUserInTempVoice(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, string task)
        {
            var tempChannels = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList(guild.Id).Result;
            var tempChannel = tempChannels.Where(ch => ch.channelid == user.VoiceChannel.Id).FirstOrDefault();
            if (tempChannel != null)
            {
                return false;
            }

            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                $"You are not connected to a temp-channel", "Not connected to temp-channel!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                message: "User not in temp-channel");

            return true;
        }

        public static async Task<bool> CheckIfUserInVoice(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, string task)
        {
            if (user.VoiceState != null)
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                $"You are not connected to a voice channel", "Not Connected!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                message: "User not in voice");

            return true;
        }

        public static async Task<bool> CheckIfUserIsOwnerOfTempChannel(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, string task)
        {
            var ownerId = TempChannel.EntityFramework.TempChannelsHelper.GetOwnerID(user.VoiceChannel.Id).Result;
            if (user.Id == ownerId)
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                $"You are not the owner of the temp-channel!\nPlease ask <@{ownerId}> to make changes!", "Not the owner!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                message: "User is not the Owner of the temp-channel");
            return true;
        }

        public static async Task<bool> CheckIfUserInSameTempVoice(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, ulong userId, DiscordSocketClient client, string task)
        {
            var tempVoiceId = user.VoiceChannel.Id;
            var usedGuild = client.GetGuild(guild.Id);

            var otherUser = usedGuild.GetUser(userId);
            if (otherUser == null)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"The given user <@{userId}> is not part of this guild!\nPlease use @user!", "Unknown User!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                    message: "User not in guild");
                return true;
            }

            if (otherUser.VoiceState == null)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"The given user <@{userId}> is not connected to any voice channel of this guild!\nPlease make sure to use a user in your channel!", "User not connected!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                    message: "User not in voice");
                return true;
            }

            if (otherUser.VoiceChannel.Id == user.VoiceChannel.Id)
            {
                return false;
            }

            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                $"The user <@{otherUser.Id}> is in a different voice channel", "Different voice channel!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                message: "User not in this temp-channel");

            return true;
        }

        public static async Task<bool> CheckIfInputIsNumber(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, string theNumberToCheck, string theThingToCheckName, string task)
        {
            if (int.TryParse(theNumberToCheck, out _))
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                $"The value of {theThingToCheckName} is not a number!\nPlease input a number!", "Not a number!").Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                message: "The given input is not number as expected", iD: theNumberToCheck);
            return true;
        }

        public static async Task<bool> CheckUserID(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, string userIdToCheck, DiscordSocketClient client, string task)
        {
            if (!(userIdToCheck.StartsWith("<@") && userIdToCheck.EndsWith(">")))
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"The given user {userIdToCheck} is not a valid user!\nPlease use @user!", "Invalid user!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                    message: "The given user is not in the right format <@number>", iD: userIdToCheck);
                return true;
            }

            userIdToCheck = userIdToCheck.Replace("<@", "");
            userIdToCheck = userIdToCheck.Replace("!", "");
            userIdToCheck = userIdToCheck.Replace(">", "");

            if (userIdToCheck.Length != 18 || !ulong.TryParse(userIdToCheck, out _))
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The given id {userIdToCheck} is not valid id!\nPlease use @user!", "Invalid Id!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                    message: "Invalid user ID", iD: userIdToCheck);
                return true;
            }

            var guildUser = client.GetUserAsync(ulong.Parse(userIdToCheck)).Result;
            if (guildUser == null)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The given id {userIdToCheck} does not belong to a user in this guild!\nPlease use @user!", "Invalid Id!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                    message: "Id does not belong to a user in this guild", iD: userIdToCheck);
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfUserInGuild(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, ulong userId, DiscordSocketClient client, string task)
        {
            var tempVoiceId = user.VoiceChannel.Id;
            var usedGuild = client.GetGuild(guild.Id);

            var otherUser = usedGuild.GetUser(userId);
            if (otherUser == null)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction,
                    $"The given user <@{userId}> is not part of this guild!\nPlease use @user!", "Unknown User!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, task, new Entities.SlashCommandParameter() { Guild = guild, GuildUser = user },
                    message: "User not in guild", iD: userId.ToString());
                return true;
            }
            return false;
        }
    }
}
