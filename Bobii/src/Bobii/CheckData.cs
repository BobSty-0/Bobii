using Bobii.src.Models;
using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    class CheckDatas
    {
        public static async Task<bool> CheckIfIDBelongsToVoiceChannel(SlashCommandParameter parameter, string Id, string task)
        {
            foreach (var channel in parameter.Guild.VoiceChannels)
            {
                if (channel.Id.ToString() == Id)
                {
                    return false;
                }
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] {
                Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Helper.GetContent("C001", parameter.Language).Result, Id),
                    Helper.GetCaption("C001", parameter.Language).Result
                ).Result
            }, ephemeral: true);

            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms,
                true, task, parameter,
                message: "ID does not belong to a voice channel");
            return true;
        }

        public static async Task<bool> DoesALogChannelExist(SlashCommandParameter parameter, string task)
        {
            if (FilterLink.EntityFramework.FilterLinkLogsHelper.DoesALogChannelExist(parameter.GuildID).Result)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                Bobii.Helper.GetContent("C052", parameter.Language).Result,
                Bobii.Helper.GetCaption("C052", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, message: $"No filterlink log channel to update");
            return true;
        }

        public static async Task<bool> CheckDiscordChannelIDFormat(SlashCommandParameter parameter, string Id, string task, bool channel)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(Id, out _) || Id.Length != 18)
            {
                if (channel)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] {
                        Helper.CreateEmbed(parameter.Interaction,
                            String.Format(Helper.GetContent("C003", parameter.Language).Result, "channel", Id),
                            String.Format(Helper.GetCaption("C003", parameter.Language).Result, "channel")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms,
                        true, task, parameter,
                        iD: Id, message: "Invalid channel ID");
                    return true;
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] {
                        Helper.CreateEmbed(parameter.Interaction,
                            String.Format(Helper.GetContent("C003", parameter.Language).Result, "guild", Id),
                            String.Format(Helper.GetCaption("C003", parameter.Language).Result, "guild")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms,
                        true, task, parameter,
                        iD: Id, message: "Invalid guild ID");
                    return true;
                }

            }
            return false;
        }

        public static async Task<bool> CheckIfCreateTempChannelWithGivenIDExists(SlashCommandParameter parameter, string createChannelID, string task)
        {
            if (TempChannel.EntityFramework.CreateTempChannelsHelper.CheckIfCreateVoiceChannelExist(parameter.Guild, ulong.Parse(createChannelID)).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] {
                    Helper.CreateEmbed(parameter.Interaction,
                        String.Format(Helper.GetContent("C004", parameter.Language).Result, createChannelID),
                        Helper.GetCaption("C004", parameter.Language).Result
                    ).Result
                }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms,
                    true, task, parameter,
                    createChannelID: ulong.Parse(createChannelID), message: $"Create temp channel with given ID does not exist!");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfCreateTempChannelWithGivenIDAlreadyExists(SlashCommandParameter parameter,
            string createChannelID, string task)
        {
            if (!TempChannel.EntityFramework.CreateTempChannelsHelper.CheckIfCreateVoiceChannelExist(parameter.Guild, ulong.Parse(createChannelID)).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] {
                    Helper.CreateEmbed(parameter.Interaction,
                        String.Format(Helper.GetContent("C006", parameter.Language).Result, createChannelID),
                        Helper.GetCaption("C006", parameter.Language).Result)
                    .Result
                }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms,
                    true, task, parameter,
                    createChannelID: ulong.Parse(createChannelID), message: "Create temp channel does not exist");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckNameLength(SlashCommandParameter parameter, string createChannelID,
            string name, string task, int lenght, bool tempchannel)
        {
            if (name.Length > lenght)
            {
                if (tempchannel)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] {
                        Helper.CreateEmbed(parameter.Interaction,
                            String.Format(Helper.GetContent("C009", parameter.Language).Result, "temp-channel-name", name, lenght),
                            String.Format( Helper.GetCaption("C009",parameter.Language).Result, "temp-channel-name")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms,
                        true, task, parameter,
                        createChannelID: ulong.Parse(createChannelID), message: "The length of the temp-channel-name is too long");
                    return true;
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] {
                        Helper.CreateEmbed(parameter.Interaction,
                            String.Format(Helper.GetContent("C009", parameter.Language).Result, "word", name, lenght),
                            String.Format(Helper.GetCaption("C009",parameter.Language).Result, "word")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                        message: "The length of the word is too long");
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> CheckLinkFormat(SlashCommandParameter parameter, string link, string task)
        {
            if (link.StartsWith("https://") || link.StartsWith("http://"))
            {
                return false;
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                Helper.GetContent("C141", parameter.Language).Result,
                Helper.GetCaption("C141", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                message: "Wrong link format", link: link);
            return true;
        }

        public static async Task<bool> IsUserAlreadyOnWhiteList(SlashCommandParameter parameter, ulong userId, string task)
        {
            if (!FilterLink.EntityFramework.FilterLinkUserGuildHelper.IsUserOnWhitelistInGuild(parameter.GuildID, userId).Result)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                String.Format( Helper.GetContent("C057",parameter.Language).Result, userId),
                Helper.GetCaption("C057", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, message: $"User already whitelisted");
            return true;
        }

        public static async Task<bool> IsUserOnWhiteList(SlashCommandParameter parameter, ulong userId, string task)
        {
            if (FilterLink.EntityFramework.FilterLinkUserGuildHelper.IsUserOnWhitelistInGuild(parameter.GuildID, userId).Result)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                String.Format( Helper.GetContent("C060",parameter.Language).Result, userId),
                Helper.GetCaption("C060",parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "FLURemove", parameter, message: $"User not on whitelist");
            return true;
        }

        public static async Task<bool> CheckIfFilterLinkIsAlreadyWhitelisted(SlashCommandParameter parameter, string link, string task)
        {
            if (!FilterLink.EntityFramework.FilterLinksGuildHelper.IsFilterlinkAllowedInGuild(parameter.GuildID, link).Result)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                String.Format( Helper.GetContent("C063",parameter.Language).Result, link),
                Helper.GetCaption("C057", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "FLLAdd", parameter, message: $"Link already on whitelist");
            return true;
        }

        public static async Task<bool> CheckIfFilterLinkOptionIsWhitelisted(SlashCommandParameter parameter, string link, string task)
        {
            if (FilterLink.EntityFramework.FilterLinksGuildHelper.IsFilterlinkAllowedInGuild(parameter.GuildID, link).Result)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format( Helper.GetContent("C070",parameter.Language).Result, link),
                    Helper.GetCaption("C070", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, message: $"FilterLink is not on whitelist");
            return true;
        }

        public static async Task<bool> CheckUserPermission(SlashCommandParameter parameter, string task)
        {
            if (parameter.GuildUser.GuildPermissions.Administrator || parameter.GuildUser.GuildPermissions.ManageGuild)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Helper.CreateEmbed(parameter.Interaction,
                String.Format(Helper.GetContent("C028", parameter.Language).Result, parameter.SlashCommandData.Name),
                Helper.GetCaption("C028", parameter.Language).Result).Result }, ephemeral: true);

            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                message: "Missing premissions");
            return true;
        }

        public static async Task<bool> CheckIfLinkIsEmojiLink(SlashCommandParameter parameter, string link, string task)
        {
            if (link.StartsWith("https://cdn.discordapp.com/emojis/"))
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                Helper.GetContent("C088", parameter.Language).Result, 
                Helper.GetCaption("C088", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "StealEmojiUrl", parameter, emojiString: link, message: "Invalid Emoji url");
            return true;
        }

        public static async Task<bool> CheckIfEmojiWithNameAlreadyExists(SlashCommandParameter parameter, string name, string task)
        {
            if (parameter.Guild.Emotes.Where(e => e.Name == name).FirstOrDefault() == null)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                Helper.GetContent("C089", parameter.Language).Result,
                Helper.GetCaption("C089", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "StealEmojiUrl", parameter, message: "Emote name already exists");
            return true;
        }

        public static async Task<bool> CheckMessageID(SlashCommandParameter parameter, string id, string task)
        {
            if (!ulong.TryParse(id, out _) || id.Length != 18)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Helper.GetContent("C142", parameter.Language).Result, id),
                    Helper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, messageID: id,
                    message: "Invalid ID");
                return true;
            }

            var channel = (SocketTextChannel)parameter.Client.GetChannel(parameter.Interaction.Channel.Id);

            var messagesInChannel = channel.GetMessagesAsync(100).Flatten();
            if (messagesInChannel == null || messagesInChannel.ToArrayAsync().Result.Count() == 0)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Helper.GetContent("C143", parameter.Language).Result,
                    Helper.GetCaption("C143", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, messageID: id,
                    message: "No message detected");
                return true;
            }

            var message = messagesInChannel.ToArrayAsync().Result.Where(m => m.Id == ulong.Parse(id)).FirstOrDefault();
            if (message == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Helper.GetContent("C144", parameter.Language).Result, id),
                    Helper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, messageID: id,
                    message: "Invalid ID");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfMessageFromCreateEmbed(SlashCommandParameter parameter, ulong messageId, string task)
        {
            var channel = (SocketTextChannel)parameter.Client.GetChannel(parameter.Interaction.Channel.Id);
            var messagesInChannel = channel.GetMessagesAsync(100).Flatten();
            var message = messagesInChannel.ToArrayAsync().Result.Where(m => m.Id == messageId).FirstOrDefault();
            // §TODO JG/220.11.2021 Check if this works
            if (!message.Author.IsBot || !(message.Author.Id == Helper.ReadBobiiConfig(ConfigKeys.ApplicationID).ToUlong()))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Helper.GetContent("C145", parameter.Language).Result, 
                    Helper.GetCaption("C145", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, messageID: messageId.ToString(),
                    message: "Message not from Bobii");
                return true;
            }

            if (message.Interaction != null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Helper.GetContent("C146", parameter.Language).Result,
                    Helper.GetCaption("C146", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, messageID: messageId.ToString(),
                    message: "Message has an Interaction attached");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfFilterLinkOptionExists(SlashCommandParameter parameter, string name, string link, string task)
        {
            if (FilterLink.EntityFramework.FilterLinkOptionsHelper.CheckIfLinkOptionExists(name, link, parameter.GuildID).Result)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                Helper.GetContent("C147", parameter.Language).Result, 
                Helper.GetCaption("C147", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, link: link,
                message: "Filter link option does not exists");
            return true;
        }

        public static async Task<bool> CheckIfFilterLinkOptionAlreadyExists(SlashCommandParameter parameter, string name, string link, string task)
        {
            if (!FilterLink.EntityFramework.FilterLinkOptionsHelper.CheckIfLinkOptionExists(name, link, parameter.GuildID).Result)
            {
                return false;
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                string.Format(Helper.GetContent("C148", parameter.Language).Result, link, name),
                Helper.GetCaption("148", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, link: link,
                message: "Filter link option already exists");
            return true;
        }

        public static async Task<bool> CheckStringLength(SlashCommandParameter parameter, string stringToCheck, int maxLenth, string parameterName, string task)
        {
            if (stringToCheck.Length < maxLenth)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                string.Format(Helper.GetContent("C149", parameter.Language).Result, parameterName, maxLenth), 
                Helper.GetCaption("C149", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, parameterName: parameterName,
                message: "Invalid length of parameter");
            return true;
        }
        public static async Task<bool> CheckStringForAlphanumericCharacters(SlashCommandParameter parameter, string stringToCheck, string task)
        {
            if (Regex.IsMatch(stringToCheck, @"^[a-zA-Z_ ]+$"))
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                Helper.GetContent("C150", parameter.Language).Result, 
                Helper.GetCaption("C150", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, parameterName: stringToCheck,
                message: "Invalid character in Emoji name");
            return true;
        }

        public static async Task<bool>CheckIfItsAEmoji(SlashCommandParameter parameter, string emoji, string task)
        {
            if (Emote.TryParse(emoji, out var emote))
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                Helper.GetContent("C092", parameter.Language).Result, 
                Helper.GetCaption("C092", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(StealEmoji), parameter, emojiString: emoji, 
                message: "Failed to convert emote string to emote");
            return true;
        }

        public static async Task<bool> CheckMinLength(SlashCommandParameter parameter, string stringToCheck, int minLength, string nameOfThingToTest, string task)
        {
            if (stringToCheck.Length > minLength)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                string.Format(Helper.GetContent("C151", parameter.Language).Result, nameOfThingToTest, minLength),
                Helper.GetCaption("C151", parameter.Language).Result).Result }, ephemeral: true) ;
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter, parameterName: stringToCheck,
                message: "Not enough caracters");
            return true;
        }

        /// <summary>
        /// Checks the given ID for lenght and number Format
        /// </summary>
        public static async Task<bool> CheckDiscordIDFormat(SlashCommandParameter parameter, string id, string task)
        {
            if (!ulong.TryParse(id, out _) || id.Length != 18)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Helper.GetContent("C010", parameter.Language).Result, id),
                    Helper.GetCaption("C010", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                    message: $"Invalid ID **{id}**");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfItsBobSty(SlashCommandParameter parameter, string task, bool errorMessage)
        {
            //False = Its me
            //True = Its not me
            if (parameter.GuildUser.Id.ToString() == (410312323409117185).ToString())
            {
                return false;
            }
            if (errorMessage)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        String.Format(Helper.GetContent("C014", parameter.Language).Result, parameter.SlashCommand.Data),
                        Helper.GetCaption("C014", parameter.Language).Result).Result }, ephemeral: true);

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                    message: $"Someone tryed to be me");
            }
            return true;
        }

        public static async Task<bool> CheckIfUserInTempVoice(SlashCommandParameter parameter, string task)
        {
            var tempChannels = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelListFromGuild(parameter.GuildID).Result;
            var tempChannel = tempChannels.Where(ch => ch.channelid == parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).FirstOrDefault();
            if (tempChannel != null)
            {
                return false;
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                Helper.GetContent("C152", parameter.Language).Result, 
                Helper.GetCaption("C152", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                message: "User not in temp-channel");

            return true;
        }

        public static async Task<bool> CheckIfUserInVoice(SlashCommandParameter parameter, string task)
        {
            if (parameter.GuildUser.VoiceState != null)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                Helper.GetContent("C153", parameter.Language).Result, 
                Helper.GetCaption("C153", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                message: "User not in voice");

            return true;
        }

        public static async Task<bool> CheckIfUserIsOwnerOfTempChannel(SlashCommandParameter parameter, string task)
        {
            var ownerId = TempChannel.EntityFramework.TempChannelsHelper.GetOwnerID(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
            if (parameter.GuildUser.Id == ownerId)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                string.Format(Helper.GetContent("C154", parameter.Language).Result, ownerId),
                Helper.GetCaption("C154", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                message: "User is not the Owner of the temp-channel");
            return true;
        }

        public static async Task<bool> CheckIfUserInSameTempVoice(SlashCommandParameter parameter, ulong userId, string task)
        {
            var tempVoiceId = parameter.GuildUser.VoiceChannel.Id;
            var usedGuild = parameter.Client.GetGuild(parameter.GuildID);

            var otherUser = usedGuild.GetUser(userId);
            if (otherUser == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Helper.GetContent("C155", parameter.Language).Result, userId),
                    Helper.GetCaption("C155", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                    message: "User not in guild");
                return true;
            }

            if (otherUser.VoiceState == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Helper.GetContent("C156", parameter.Language).Result, userId),
                    Helper.GetCaption("C156", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                    message: "User not in voice");
                return true;
            }

            if (otherUser.VoiceChannel.Id == parameter.GuildUser.VoiceChannel.Id)
            {
                return false;
            }

            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                string.Format(Helper.GetCommandDescription("157", parameter.Language).Result, otherUser.Id),
                Helper.GetCaption("C157", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                message: "User not in this temp-channel");

            return true;
        }

        public static async Task<bool> CheckUserID(SlashCommandParameter parameter, string userIdToCheck, string task, bool withFormatting = false)
        {
            if (withFormatting)
            {
                if (!(userIdToCheck.StartsWith("<@") && userIdToCheck.EndsWith(">")))
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        string.Format(Helper.GetContent("C158", parameter.Language).Result, userIdToCheck),
                    Helper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                        message: "The given user is not in the right format <@number>", iD: userIdToCheck);
                    return true;
                }
            }

            userIdToCheck = userIdToCheck.Replace("<@", "");
            userIdToCheck = userIdToCheck.Replace("!", "");
            userIdToCheck = userIdToCheck.Replace(">", "");

            if (userIdToCheck.Length != 18 || !ulong.TryParse(userIdToCheck, out _))
            {
                if (withFormatting)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        string.Format(Helper.GetContent("C159", parameter.Language).Result, userIdToCheck),
                        Helper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        string.Format(Helper.GetContent("C160", parameter.Language).Result, userIdToCheck),
                        Helper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                }

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                    message: "Invalid user ID", iD: userIdToCheck);
                return true;
            }

            var guildUser = parameter.Client.GetUserAsync(ulong.Parse(userIdToCheck)).Result;
            if (guildUser == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Helper.GetContent("C161", parameter.Language).Result, userIdToCheck),
                    Helper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                    message: "Id does not belong to a user in this guild", iD: userIdToCheck);
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfUserInGuild(SlashCommandParameter parameter, ulong userId, string task)
        {
            var tempVoiceId = parameter.GuildUser.VoiceChannel.Id;
            var usedGuild = parameter.Client.GetGuild(parameter.GuildID);

            var otherUser = usedGuild.GetUser(userId);
            if (otherUser == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Helper.GetContent("C162", parameter.Language).Result, userId),
                    Helper.GetCaption("C155", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                    message: "User not in guild", iD: userId.ToString());
                return true;
            }
            return false;
        }
    }
}
