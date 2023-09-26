using Bobii.src.Helper;
using Bobii.src.Models;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Reflection.Metadata;
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
                GeneralHelper.CreateEmbed(parameter.Interaction,
                    String.Format(GeneralHelper.GetContent("C001", parameter.Language).Result, Id),
                    GeneralHelper.GetCaption("C001", parameter.Language).Result
                ).Result
            }, ephemeral: true);

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms,
                true, task, parameter,
                message: "ID does not belong to a voice channel");
            return true;
        }

        public static async Task<bool> CheckIfCommandIsDisabled(SlashCommandParameter parameter, string command, ulong createchannelid, bool epherialMessage = false)
        {
            if(TempCommandsHelper.DoesCommandExist(parameter.GuildID, createchannelid,command).Result)
            {
                if (epherialMessage)
                {
                    var parsedArg = (SocketMessageComponent)parameter.Interaction;
                    await parsedArg.UpdateAsync(msg => {
                        msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            String.Format(GeneralHelper.GetContent("C186", parameter.Language).Result, $"/temp {command}"),
                            GeneralHelper.GetCaption("C186", parameter.Language).Result).Result };
                        msg.Components = null;
                    });
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] {
                        GeneralHelper.CreateEmbed(parameter.Interaction,
                            String.Format(GeneralHelper.GetContent("C186", parameter.Language).Result, $"/temp {command}"),
                            GeneralHelper.GetCaption("C186", parameter.Language).Result
                        ).Result
                    }, ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms,
                    true, command,parameter,
                    message: "Command disabled");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckDiscordChannelIDFormat(SlashCommandParameter parameter, string Id, string task, bool channel)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(Id, out _) || (Id.Length < 17 && Id.Length > 20))
            {
                if (channel)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] {
                        GeneralHelper.CreateEmbed(parameter.Interaction,
                            String.Format(GeneralHelper.GetContent("C003", parameter.Language).Result, "channel", Id),
                            String.Format(GeneralHelper.GetCaption("C003", parameter.Language).Result, "channel")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms,
                        true, task, parameter,
                        iD: Id, message: "Invalid channel ID");
                    return true;
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] {
                        GeneralHelper.CreateEmbed(parameter.Interaction,
                            String.Format(GeneralHelper.GetContent("C003", parameter.Language).Result, "guild", Id),
                            String.Format(GeneralHelper.GetCaption("C003", parameter.Language).Result, "guild")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms,
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
                    GeneralHelper.CreateEmbed(parameter.Interaction,
                        String.Format(GeneralHelper.GetContent("C004", parameter.Language).Result, createChannelID),
                        GeneralHelper.GetCaption("C004", parameter.Language).Result
                    ).Result
                }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms,
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
                    GeneralHelper.CreateEmbed(parameter.Interaction,
                        String.Format(GeneralHelper.GetContent("C006", parameter.Language).Result, createChannelID),
                        GeneralHelper.GetCaption("C006", parameter.Language).Result)
                    .Result
                }, ephemeral: true);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms,
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
                        GeneralHelper.CreateEmbed(parameter.Interaction,
                            String.Format(GeneralHelper.GetContent("C009", parameter.Language).Result, "temp-channel-name", name, lenght),
                            String.Format( GeneralHelper.GetCaption("C009",parameter.Language).Result, "temp-channel-name")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms,
                        true, task, parameter,
                        createChannelID: ulong.Parse(createChannelID), message: "The length of the temp-channel-name is too long");
                    return true;
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] {
                        GeneralHelper.CreateEmbed(parameter.Interaction,
                            String.Format(GeneralHelper.GetContent("C009", parameter.Language).Result, "word", name, lenght),
                            String.Format(GeneralHelper.GetCaption("C009",parameter.Language).Result, "word")
                        ).Result
                    }, ephemeral: true);

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                        message: "The length of the word is too long");
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> CheckUserPermission(SlashCommandParameter parameter, string task)
        {
            if (parameter.GuildUser.GuildPermissions.Administrator || parameter.GuildUser.GuildPermissions.ManageGuild)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                String.Format(GeneralHelper.GetContent("C028", parameter.Language).Result, parameter.SlashCommandData.Name),
                GeneralHelper.GetCaption("C028", parameter.Language).Result).Result }, ephemeral: true);

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                message: "Missing premissions");
            return true;
        }

        public static async Task<bool> CheckIfLinkIsEmojiLink(SlashCommandParameter parameter, string link, string task)
        {
            if (link.StartsWith("https://cdn.discordapp.com/emojis/"))
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction, 
                GeneralHelper.GetContent("C088", parameter.Language).Result, 
                GeneralHelper.GetCaption("C088", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, "StealEmojiUrl", parameter, emojiString: link, message: "Invalid Emoji url");
            return true;
        }

        public static async Task<bool> CheckIfEmojiWithNameAlreadyExists(SlashCommandParameter parameter, string name, string task)
        {
            if (parameter.Guild.Emotes.Where(e => e.Name == name).FirstOrDefault() == null)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction, 
                GeneralHelper.GetContent("C089", parameter.Language).Result,
                GeneralHelper.GetCaption("C089", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, "StealEmojiUrl", parameter, message: "Emote name already exists");
            return true;
        }

        public static async Task<bool> CheckMessageID(SlashCommandParameter parameter, string id, string task)
        {
            if (!ulong.TryParse(id, out _) || (id.Length < 17 || id.Length > 20))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C142", parameter.Language).Result, id),
                    GeneralHelper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter, messageID: id,
                    message: "Invalid ID");
                return true;
            }

            var channel = (SocketTextChannel)parameter.Client.GetChannel(parameter.Interaction.Channel.Id);

            var messagesInChannel = channel.GetMessagesAsync(100).Flatten();
            if (messagesInChannel == null || messagesInChannel.ToArrayAsync().Result.Count() == 0)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C143", parameter.Language).Result,
                    GeneralHelper.GetCaption("C143", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter, messageID: id,
                    message: "No message detected");
                return true;
            }

            var message = messagesInChannel.ToArrayAsync().Result.Where(m => m.Id == ulong.Parse(id)).FirstOrDefault();
            if (message == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C144", parameter.Language).Result, id),
                    GeneralHelper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter, messageID: id,
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
            if (!message.Author.IsBot || !(message.Author.Id == GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationID).ToUlong()))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C145", parameter.Language).Result, 
                    GeneralHelper.GetCaption("C145", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter, messageID: messageId.ToString(),
                    message: "Message not from Bobii");
                return true;
            }

            if (message.Interaction != null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C146", parameter.Language).Result,
                    GeneralHelper.GetCaption("C146", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter, messageID: messageId.ToString(),
                    message: "Message has an Interaction attached");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckStringLength(SlashCommandParameter parameter, string stringToCheck, int maxLenth, string parameterName, string task)
        {
            if (stringToCheck.Length < maxLenth)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                string.Format(GeneralHelper.GetContent("C149", parameter.Language).Result, parameterName, maxLenth), 
                GeneralHelper.GetCaption("C149", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter, parameterName: parameterName,
                message: "Invalid length of parameter");
            return true;
        }

        public static async Task<bool> CheckStringForAlphanumericCharacters(SlashCommandParameter parameter, string stringToCheck, string task)
        {
            if (Regex.IsMatch(stringToCheck, @"^[a-zA-Z_0-9]+$"))
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction, 
                GeneralHelper.GetContent("C150", parameter.Language).Result, 
                GeneralHelper.GetCaption("C150", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter, parameterName: stringToCheck,
                message: "Invalid character in Emoji name");
            return true;
        }

        public static async Task<bool>CheckIfItsAEmoji(SlashCommandParameter parameter, string emoji, string task)
        {
            if (Emote.TryParse(emoji, out var emote))
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction, 
                GeneralHelper.GetContent("C092", parameter.Language).Result, 
                GeneralHelper.GetCaption("C092", parameter.Language).Result).Result }, ephemeral: true);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(CheckIfItsAEmoji), parameter, emojiString: emoji, 
                message: "Failed to convert emote string to emote");
            return true;
        }

        public static async Task<bool> CheckMinLength(SlashCommandParameter parameter, string stringToCheck, int minLength, string nameOfThingToTest, string task)
        {
            if (stringToCheck.Length >= minLength)
            {
                return false;
            }
            await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                string.Format(GeneralHelper.GetContent("C151", parameter.Language).Result, nameOfThingToTest, minLength),
                GeneralHelper.GetCaption("C151", parameter.Language).Result).Result }, ephemeral: true) ;
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter, parameterName: stringToCheck,
                message: "Not enough caracters");
            return true;
        }

        public static async Task<bool> CheckIfUserInTempVoice(SlashCommandParameter parameter, string task, bool epherialMessage = false)
        {

            var tempChannels = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelListFromGuild(parameter.GuildID).Result;
            var tempChannel = tempChannels.Where(ch => ch.channelid == parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).FirstOrDefault();
            if (tempChannel != null)
            {
                return false;
            }

            if (epherialMessage)
            {
                var parsedArg = (SocketMessageComponent)parameter.Interaction;
                await parsedArg.UpdateAsync(msg => {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C152", parameter.Language).Result,
                        GeneralHelper.GetCaption("C152", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C152", parameter.Language).Result,
                    GeneralHelper.GetCaption("C152", parameter.Language).Result).Result }, ephemeral: true);
            }

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                message: "User not in temp-channel");

            return true;
        }

        public static async Task<bool> UserTempChannelConfigExists(SlashCommandParameter parameter)
        {
            var tempChannels = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelListFromGuild(parameter.GuildID).Result;
            var tempChannel = tempChannels.Where(ch => ch.channelid == parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).FirstOrDefault();

            var tempChannelConfigs = TempChannel.EntityFramework.TempChannelUserConfig.GetTempChannelConfigs(parameter.GuildID).Result;
            var usersTempChannelConfig = tempChannelConfigs.SingleOrDefault(t => t.createchannelid == tempChannel.createchannelid && t.userid == parameter.GuildUser.Id);

            return usersTempChannelConfig != null;
        }

        public static async Task<bool> CheckIfUserTempChannelConfigExists(SlashCommandParameter parameter, string task)
        {
            if (UserTempChannelConfigExists(parameter).Result)
            {
                return false;
            }

            await parameter.Interaction.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                             GeneralHelper.GetContent("C179", parameter.Language).Result,
                             GeneralHelper.GetCaption("C179", parameter.Language).Result).Result };
                msg.Components = null;
            });

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                message: "User has no temp-channel config");
            return true;
        }

        public static async Task<bool> CheckIfUserInVoice(SlashCommandParameter parameter, string task, bool epherialMessage = false)
        {
            if (parameter.GuildUser.VoiceState != null)
            {
                return false;
            }

            if (epherialMessage)
            {
                var parsedArg = (SocketMessageComponent)parameter.Interaction;
                await parsedArg.UpdateAsync(msg => {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        GeneralHelper.GetContent("C153", parameter.Language).Result,
                        GeneralHelper.GetCaption("C153", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C153", parameter.Language).Result,
                    GeneralHelper.GetCaption("C153", parameter.Language).Result).Result }, ephemeral: true);
            }
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                message: "User not in voice");

            return true;
        }

        public static async Task<bool> CheckIfUserIsOwnerOfTempChannel(SlashCommandParameter parameter, string task,  bool epherialMessage = false, bool checkForModerator = true)
        {
            var ownerId = TempChannel.EntityFramework.TempChannelsHelper.GetOwnerID(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;
            if (parameter.GuildUser.Id == ownerId)
            {
                return false;
            }

            if (checkForModerator && UsedFunctionsHelper.GetUsedFunction(ownerId, parameter.GuildUser.Id, GlobalStrings.moderator, parameter.Guild.Id).Result != null)
            {
                return false;
            }

            if (epherialMessage)
            {
                var parsedArg = (SocketMessageComponent)parameter.Interaction;
                await parsedArg.UpdateAsync(msg => {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        string.Format(GeneralHelper.GetContent("C154", parameter.Language).Result, ownerId),
                        GeneralHelper.GetCaption("C154", parameter.Language).Result).Result };
                    msg.Components = null;
                });
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                string.Format(GeneralHelper.GetContent("C154", parameter.Language).Result, ownerId),
                GeneralHelper.GetCaption("C154", parameter.Language).Result).Result }, ephemeral: true);
            }

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                message: "User is not the Owner of the temp-channel");
            return true;
        }

        public static async Task<string> CheckIfEvenModString(SlashCommandParameter parameter, ulong userId)
        {
            if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, userId, GlobalStrings.moderator, parameter.Guild.Id).Result == null)
            {
                return GeneralHelper.GetContent("C304", parameter.Language).Result;
            }
            return "";
        }

        public static async Task<string> CheckIfAlreadyModString(SlashCommandParameter parameter, ulong userId, int maxNumber)
        {
            var tempChannel = TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

            if (UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, userId, GlobalStrings.moderator, parameter.Guild.Id).Result != null ||
                tempChannel.channelownerid == userId)
            {
                return GeneralHelper.GetContent("C303", parameter.Language).Result;
            }

            if(UsedFunctionsHelper.GetUsedFunction(parameter.GuildUser.Id, userId, GlobalStrings.block, parameter.Guild.Id).Result != null)
            {
                return GeneralHelper.GetContent("C292", parameter.Language).Result;
            }

            if (UsedFunctionsHelper.GetAllModeratorsFromUser(parameter.GuildUser.Id, parameter.GuildID).Result.Count() == maxNumber)
            {
                return String.Format(GeneralHelper.GetContent("C307", parameter.Language).Result, maxNumber);
            }
            return "";
        }

        public static async Task<string> CheckIfUserInSameTempVoiceString(SlashCommandParameter parameter, ulong userId, string taskSprachCode)
        {
            var tempVoiceId = parameter.GuildUser.VoiceChannel.Id;
            var otherUser = parameter.Guild.GetUser(userId);

            // User not in a voice channel
            if (otherUser.VoiceState == null)
            {
                return GeneralHelper.GetContent("C251", parameter.Language).Result;
            }

            // User not in this voice
            if (otherUser.VoiceChannel.Id != parameter.GuildUser.VoiceChannel.Id)
            {
                return GeneralHelper.GetContent("C251", parameter.Language).Result;
            }

            var permissionString = CheckPermissionsString(parameter, userId, taskSprachCode).Result;
            if (permissionString != "")
            {
                return permissionString;
            }
            return "";
        }

        public static async Task<string> CheckPermissionsString(SlashCommandParameter parameter, ulong userId, string taskSprachCode, bool checkPermission = true)
        {
            var tempVoiceId = parameter.GuildUser.VoiceChannel.Id;
            var otherUser = parameter.Guild.GetUser(userId);

            // Cant kick yourself
            if (otherUser.Id == parameter.GuildUser.Id)
            {
                return String.Format(GeneralHelper.GetContent("C256", parameter.Language).Result, GeneralHelper.GetCaption(taskSprachCode, parameter.Language).Result);
            }

            if (otherUser.Id == ulong.Parse(GeneralHelper.GetConfigKeyValue(ConfigKeys.ApplicationID)))
            {
                return string.Format(GeneralHelper.GetContent("C265", parameter.Language).Result);
            }

            var tempChannel = TempChannelsHelper.GetTempChannel(tempVoiceId).Result;

            // Cant kick the owner
            if (otherUser.Id == tempChannel.channelownerid)
            {
                return String.Format(GeneralHelper.GetContent("C257", parameter.Language).Result, GeneralHelper.GetCaption(taskSprachCode, parameter.Language).Result);
            }

            if (!checkPermission)
            {
                return "";
            }

            // Cant kick admin
            if (otherUser.GuildPermissions.Administrator)
            {
                return GeneralHelper.GetContent("C254", parameter.Language).Result;
            }

            // Cant kick Manage  Server rights
            if (otherUser.GuildPermissions.ManageGuild)
            {
                return GeneralHelper.GetContent("C255", parameter.Language).Result;
            }

            return "";
        }

        public static async Task<bool> CheckIfUserInSameTempVoice(SlashCommandParameter parameter, ulong userId, string task, bool epherialMessage = false)
        {
            var tempVoiceId = parameter.GuildUser.VoiceChannel.Id;
            var usedGuild = parameter.Client.GetGuild(parameter.GuildID);

            var otherUser = usedGuild.GetUser(userId);
            if (otherUser == null)
            {
                if (epherialMessage)
                {
                    var parsedArg = (SocketMessageComponent)parameter.Interaction;
                    await parsedArg.UpdateAsync(msg => {
                        msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            string.Format(GeneralHelper.GetContent("C155", parameter.Language).Result, userId),
                            GeneralHelper.GetCaption("C155", parameter.Language).Result).Result };
                        msg.Components = null;
                    });
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, 
                        new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            string.Format(GeneralHelper.GetContent("C155", parameter.Language).Result, userId),
                            GeneralHelper.GetCaption("C155", parameter.Language).Result).Result }, 
                        ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                    message: "User not in guild");
                return true;
            }

            if (otherUser.VoiceState == null)
            {
                if (epherialMessage)
                {
                    var parsedArg = (SocketMessageComponent)parameter.Interaction;
                    await parsedArg.UpdateAsync(msg => {
                        msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            string.Format(GeneralHelper.GetContent("C156", parameter.Language).Result, userId),
                            GeneralHelper.GetCaption("C156", parameter.Language).Result).Result };
                        msg.Components = null;
                    });
                }
                else
                {
                    await parameter.Interaction.RespondAsync(
                        null, 
                        new Embed[] { 
                            GeneralHelper.CreateEmbed(parameter.Interaction,
                            string.Format(GeneralHelper.GetContent("C156", parameter.Language).Result, userId),
                            GeneralHelper.GetCaption("C156", parameter.Language).Result).Result 
                        }, 
                        ephemeral: true);
                }


                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                    message: "User not in voice");
                return true;
            }

            if (otherUser.VoiceChannel.Id == parameter.GuildUser.VoiceChannel.Id)
            {
                return false;
            }

            if (epherialMessage)
            {
                var parsedArg = (SocketMessageComponent)parameter.Interaction;
                await parsedArg.UpdateAsync(msg => {
                    msg.Embeds = new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        string.Format(GeneralHelper.GetContent("C157", parameter.Language).Result, otherUser.Id),
                        GeneralHelper.GetCaption("C157", parameter.Language).Result).Result  };
                    msg.Components = null;
                });
            }
            else
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                string.Format(GeneralHelper.GetContent("C157", parameter.Language).Result, otherUser.Id),
                GeneralHelper.GetCaption("C157", parameter.Language).Result).Result }, ephemeral: true);
            }

            await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                message: "User not in this temp-channel");

            return true;
        }

        public static async Task<bool> CheckUserID(SlashCommandParameter parameter, string userIdToCheck, string task, bool withFormatting = false)
        {
            if (withFormatting)
            {
                if (!(userIdToCheck.StartsWith("<@") && userIdToCheck.EndsWith(">")))
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        string.Format(GeneralHelper.GetContent("C158", parameter.Language).Result, userIdToCheck),
                    GeneralHelper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, task, parameter,
                        message: "The given user is not in the right format <@number>", iD: userIdToCheck);
                    return true;
                }
            }

            userIdToCheck = userIdToCheck.Replace("<@", "");
            userIdToCheck = userIdToCheck.Replace("!", "");
            userIdToCheck = userIdToCheck.Replace(">", "");

            if ((userIdToCheck.Length < 17 || userIdToCheck.Length > 20) || !ulong.TryParse(userIdToCheck, out _))
            {
                if (withFormatting)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        string.Format(GeneralHelper.GetContent("C159", parameter.Language).Result, userIdToCheck),
                        GeneralHelper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                }
                else
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                        string.Format(GeneralHelper.GetContent("C160", parameter.Language).Result, userIdToCheck),
                        GeneralHelper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                    message: "Invalid user ID", iD: userIdToCheck);
                return true;
            }

            var guildUser = parameter.Client.GetUserAsync(ulong.Parse(userIdToCheck)).Result;
            if (guildUser == null)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C161", parameter.Language).Result, userIdToCheck),
                    GeneralHelper.GetCaption("C142", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
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
                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C162", parameter.Language).Result, userId),
                    GeneralHelper.GetCaption("C155", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, task, parameter,
                    message: "User not in guild", iD: userId.ToString());
                return true;
            }
            return false;
        }
    }
}
