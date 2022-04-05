using Bobii.src.Models;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class SlashCommands
    {
        #region Info
        public static async Task TCInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCInfo)).Result)
            {
                return;
            }
            await parameter.Interaction.RespondAsync("", new Embed[] { Helper.CreateVoiceChatInfoEmbed(parameter) });
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCInfo), parameter, message: "/tcinfo successfully used");
        }

        public static async Task TCCreateInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCCreateInfo)).Result)
            {
                return;
            }
            await parameter.Interaction.Channel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(parameter.Interaction,
                Helper.HelpEditTempChannelInfoPart(parameter.Client.Rest.GetGlobalApplicationCommands().Result, parameter.GuildID, true).Result,
                Bobii.Helper.GetContent("C106", parameter.Language).Result).Result);

            await parameter.Interaction.DeferAsync();
            await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCCreateInfo), parameter, message: "/tccreateinfo successfully used");
        }
        #endregion

        #region Utility
        public static async Task TCAdd(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCAdd)).Result)
            {
                return;
            }
            bool textChannelb = false;

            var createVoiceChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "createvoicechannel").Result.String;
            var tempChannelName = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "tempchannelname").Result.String;
            var channelSize = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "channelsize").Result.Integer;
            var textChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "textchannel").Result.String;
            var delay = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "delay").Result.Integer;

            int? finalDelay = null;
            if (delay != 0)
            {
                finalDelay = delay;
            }


            var nameAndID = createVoiceChannel.Split(" ");


            if (createVoiceChannel == Bobii.Helper.GetContent("C095", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C107", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C107", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCAdd), parameter, message: "Could not find any channels");
                return;
            }

            var createChannelID = nameAndID[nameAndID.Count() - 1];

            if (textChannel != null)
            {
                if (textChannel == "on")
                {
                    textChannelb = true;
                }
            }

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, nameof(TCAdd), true).Result ||
                Bobii.CheckDatas.CheckIfIDBelongsToVoiceChannel(parameter, createChannelID, nameof(TCAdd)).Result ||
                Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDExists(parameter, createChannelID, nameof(TCAdd)).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter, createChannelID, tempChannelName, nameof(TCAdd), 50, true).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.CreateTempChannelsHelper.AddCC(parameter.GuildID, tempChannelName, ulong.Parse(createChannelID), channelSize, textChannelb, finalDelay);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C108", parameter.Language).Result, parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name, parameter.GuildUser.Username),
                    Bobii.Helper.GetCaption("C108", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCAdd), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "/tcadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C109", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCAdd), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "Failed to add CreateTempChannel", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task TCUpdate(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCUpdate)).Result)
            {
                return;
            }

            bool textChannelNewb = false;
            var sb = new StringBuilder();
            var createVoiceChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "createvoicechannel").Result.String;
            var newTempChannelName = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newtempchannelname").Result.String;
            var newChannelSize = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newtempchannelsize").Result.Integer;
            var newTextChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "textchannel").Result.String;
            var delay = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "delay").Result.Integer;

            int? finalDelay = null;
            if (delay != 0)
            {
                finalDelay = delay;
            }

            var nameAndID = createVoiceChannel.Split(" ");
            var createChannelID = nameAndID[nameAndID.Count() - 1];

            if (createVoiceChannel == Bobii.Helper.GetContent("C096", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C110", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C110", parameter.Language).Result).Result },
                    ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCUpdate), parameter, message: "Could not find any channels");
                return;
            }

            // Only the create-temp-channel was choosen
            if (parameter.SlashCommandData.Options.Count() == 1)
            {
                return;
            }

            if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, nameof(TCUpdate), true).Result ||
                 Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createChannelID, nameof(TCUpdate)).Result)
            {
                return;
            }

            try
            {
                if (newTempChannelName != null)
                {
                    if (Bobii.CheckDatas.CheckNameLength(parameter, createChannelID, newTempChannelName, nameof(TCUpdate), 50, true).Result)
                    {
                        return;
                    }

                    await EntityFramework.CreateTempChannelsHelper.ChangeTempChannelName(newTempChannelName, ulong.Parse(createChannelID));
                    sb.AppendLine();
                    sb.AppendLine(string.Format(Bobii.Helper.GetContent("C111", parameter.Language).Result, newTempChannelName));
                }

                if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "newtempchannelsize").FirstOrDefault() != null)
                {
                    await EntityFramework.CreateTempChannelsHelper.ChangeTempChannelSize(newChannelSize, ulong.Parse(createChannelID));
                    sb.AppendLine();
                    sb.AppendLine(string.Format(Bobii.Helper.GetContent("C112", parameter.Language).Result, newChannelSize));
                }

                if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "textchannel").FirstOrDefault() != null)
                {
                    if (newTextChannel == "on")
                    {
                        textChannelNewb = true;
                    }
                    await EntityFramework.CreateTempChannelsHelper.ChangeTextChannel(textChannelNewb, ulong.Parse(createChannelID));
                    sb.AppendLine();
                    sb.AppendLine(string.Format(Bobii.Helper.GetContent("C113", parameter.Language).Result, newTextChannel));
                }

                if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "delay").FirstOrDefault() != null)
                {
                    await EntityFramework.CreateTempChannelsHelper.ChangeDelay(finalDelay, ulong.Parse(createChannelID));
                    sb.AppendLine();
                    sb.AppendLine($"Delay successfully changed to {delay} minutes.");
                }

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    sb.ToString(),
                    Bobii.Helper.GetCaption("C111", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCUpdate), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "/tcupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C114", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCUpdate), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "Failed to update create-temp-channel", exceptionMessage: ex.Message);
                return;
            }

            await Task.CompletedTask;
        }

        public static async Task TCRemove(SlashCommandParameter parameter)
        {
            var createVoiceChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "createvoicechannel").Result.String;
            var nameAndID = createVoiceChannel.Split(" ");
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCRemove)).Result)
            {
                return;
            }
            if (createVoiceChannel == Bobii.Helper.GetContent("C096", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C115", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C115", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCRemove), parameter, message: "Could not find any channels");
                return;
            }

            var createChannelID = nameAndID[nameAndID.Count() - 1];

            if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, nameof(TCRemove), true).Result ||
                Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createChannelID, nameof(TCRemove)).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.CreateTempChannelsHelper.RemoveCC(parameter.GuildID.ToString(), ulong.Parse(createChannelID));
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C116", parameter.Language).Result, parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name, parameter.GuildUser.Username),
                    Bobii.Helper.GetCaption("C116", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCRemove), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "/tcremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C117", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCRemove), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "Failed to remove CreateTempChannel", exceptionMessage: ex.Message);
                return;
            }
        }
        #endregion

        #region EditChannel
        public static async Task TempName(SlashCommandParameter parameter)
        {
            var newName = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newname").Result.String;

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            try
            {
                if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempName)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempName)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempName)).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter, newName, 50, "the channel name", nameof(TempName)).Result)
                {
                    return;
                }

                _ = Task.Run(async () => parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.Name = newName));

                var tempChannel = EntityFramework.TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
                if (tempChannel.textchannelid != 0)
                {
                    var textChannel = parameter.Client.Guilds
                        .SelectMany(g => g.Channels)
                        .FirstOrDefault(c => c.Id == tempChannel.textchannelid);

                    if (textChannel != null)
                    {
                        await textChannel.ModifyAsync(channel => channel.Name = newName);
                    }
                }

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C118", parameter.Language).Result, newName),
                    Bobii.Helper.GetCaption("C118", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempName), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempname successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempName), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel name", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C119", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                return;
            }
        }

        public static async Task TempSize(SlashCommandParameter parameter)
        {
            var newSize = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newsize").Result.Integer;

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempSize)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempSize)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempSize)).Result)
            {
                return;
            }

            try
            {
                if (newSize > 99)
                {
                    _ = parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = null);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Bobii.Helper.GetContent("C120", parameter.Language).Result,
                        Bobii.Helper.GetCaption("C120", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempSize), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempsize successfully used");
                }
                else
                {
                    _ = Task.Run(async () => parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = newSize));
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        string.Format(Bobii.Helper.GetContent("C121", parameter.Language).Result, newSize),
                        Bobii.Helper.GetCaption("C121", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempSize), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempsize successfully used");
                }


            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempSize), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel size", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C122", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                return;
            }
        }

        public static async Task TempOwner(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newowner").Result.String;
            if (user == Bobii.Helper.GetContent("C094", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C123", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C123", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempOwner), parameter, message: "Could not find any users");
                return;
            }

            user = user.Split(' ')[user.Split(' ').Count() - 1];

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckUserID(parameter, user, nameof(TempOwner)).Result)
            {
                return;
            }
            user = user.Replace("<@", "");
            user = user.Replace("!", "");
            user = user.Replace(">", "");
            var userId = ulong.Parse(user);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempOwner)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempOwner)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempOwner)).Result ||
                Bobii.CheckDatas.CheckIfUserInSameTempVoice(parameter, userId, nameof(TempOwner)).Result)
            {
                return;
            }

            try
            {
                var tempChannel = EntityFramework.TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                var currentOwner = parameter.Client.GetUser(tempChannel.channelownerid.Value);
                var newOwner = parameter.Client.GetUser(ulong.Parse(user));

                if (newOwner.IsBot)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Bobii.Helper.GetContent("C124", parameter.Language).Result,
                        Bobii.Helper.GetCaption("C124", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempOwner), new SlashCommandParameter() { Guild = parameter.Guild, GuildUser = parameter.GuildUser },
                        message: "User is a Bot");
                    return;
                }

                var voiceChannel = parameter.Client.GetChannel(tempChannel.channelid);
                await Helper.RemoveManageChannelRightsToUserVc(currentOwner, voiceChannel as SocketVoiceChannel);

                if (tempChannel.textchannelid != 0)
                {
                    var textChannel = parameter.Client.Guilds
                        .SelectMany(g => g.Channels)
                        .SingleOrDefault(c => c.Id == tempChannel.textchannelid);
                    await Helper.RemoveManageChannelRightsToUserTc(currentOwner, textChannel as SocketTextChannel);

                    await Helper.GiveManageChannelRightsToUserTc(newOwner, null, textChannel as SocketTextChannel);
                }

                await Helper.GiveManageChannelRightsToUserVc(newOwner, null, voiceChannel as SocketVoiceChannel);


                await EntityFramework.TempChannelsHelper.ChangeOwner(parameter.GuildUser.VoiceChannel.Id, userId);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C125", parameter.Language).Result, userId),
                    Bobii.Helper.GetCaption("C125", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempowner successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempOwner), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel owner", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C126", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                return;
            }
        }

        public static async Task TempKick(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "user").Result.String;

            if (user == Bobii.Helper.GetContent("C093", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C127", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C127", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempKick), parameter, message: "Could not find any users");
                return;
            }

            user = user.Split(' ')[user.Split(' ').Count() - 1];
            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckUserID(parameter, user, nameof(TempKick)).Result)
            {
                return;
            }
            user = user.Replace("<@", "");
            user = user.Replace("!", "");
            user = user.Replace(">", "");
            var userId = ulong.Parse(user);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempKick)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempKick)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempKick)).Result ||
                Bobii.CheckDatas.CheckIfUserInSameTempVoice(parameter, userId, nameof(TempKick)).Result)
            {
                return;
            }

            var usedGuild = parameter.Client.GetGuild(parameter.Guild.Id);

            var toBeKickedUser = usedGuild.GetUser(userId);
            try
            {
                await toBeKickedUser.ModifyAsync(channel => channel.Channel = null);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C128", parameter.Language).Result, toBeKickedUser.Id),
                    Bobii.Helper.GetCaption("C128", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempKick), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempkick successfully used");

            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempKick), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to kick temp-channel user", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C129", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempHide(SlashCommandParameter parameter)
        {
            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock)).Result)
            {
                return;
            }

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == Bobii.Helper.ReadBobiiConfig(Bobii.ConfigKeys.ApplicationName)).First();

                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));

                //Permissions for each role
                foreach (var role in parameter.Guild.Roles)
                {
                    var permissionOverride = parameter.GuildUser.VoiceState.Value.VoiceChannel.GetPermissionOverwrite(role);
                    if (permissionOverride != null)
                    {
                        if (role.Name == Bobii.Helper.ReadBobiiConfig(Bobii.ConfigKeys.ApplicationName))
                        {
                            continue;
                        }
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value.Modify(viewChannel: PermValue.Deny)));
                    }
                    else if (role.Name == "@everyone" && permissionOverride == null)
                    {
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
                    }
                }
                var tempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id);
                await tempChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C164", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C158", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temphide successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to hide temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C165", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }

        }

        public static async Task TempUnHide(SlashCommandParameter parameter)
        {
            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock)).Result)
            {
                return;
            }

            var tempChannel = EntityFramework.TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceState.Value.VoiceChannel.Id).Result;

            var createTempChannel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.createchannelid.Value);

            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                SocketRole bobiiRole = parameter.Guild.Roles.Where(role => role.Name == Bobii.Helper.ReadBobiiConfig(Bobii.ConfigKeys.ApplicationName)).First();

                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));

                //Permissions for each role
                foreach (var role in parameter.Guild.Roles)
                {

                    var permissionOverride = createTempChannel.GetPermissionOverwrite(role);
                    if (permissionOverride != null)
                    {
                        if (role.Name == Bobii.Helper.ReadBobiiConfig(Bobii.ConfigKeys.ApplicationName))
                        {
                            continue;
                        }
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value));
                    }
                }

                await parameter.GuildUser.VoiceState.Value.VoiceChannel.ModifyAsync(v => v.PermissionOverwrites = permissions);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C166", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C166", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/temphide successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnHide), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unhide temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C167", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }

        }

        public static async Task TempLock(SlashCommandParameter parameter)
        {
            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock)).Result)
            {
                return;
            }

            try
            {
                var everyoneRole = parameter.Guild.Roles.Where(role => role.Name == "@everyone").First();
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                var newPermissionOverride = new OverwritePermissions(connect: PermValue.Deny);
                var test = voiceChannel.AddPermissionOverwriteAsync(everyoneRole, newPermissionOverride);


                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C130", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C130", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/templock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to lock temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C131", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }


        public static async Task TempUnLock(SlashCommandParameter parameter)
        {
            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnLock)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnLock)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnLock)).Result)
            {
                return;
            }

            try
            {
                var everyoneRole = parameter.Guild.Roles.Where(role => role.Name == "@everyone").First();
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                var newPermissionOverride = new OverwritePermissions(connect: PermValue.Allow);
                await voiceChannel.AddPermissionOverwriteAsync(everyoneRole, newPermissionOverride);


                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C132", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C132", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunlock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unlock temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C133", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempBlock(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "user").Result.IUser;

            await Helper.GiveOwnerIfOwnerIDZero(parameter);
            var userId = user.Id;

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempLock)).Result ||
                Bobii.CheckDatas.CheckIfUserInGuild(parameter, userId, nameof(TempLock)).Result)
            {
                return;
            }

            try
            {
                var newPermissionOverride = new OverwritePermissions().Modify(connect: PermValue.Deny, viewChannel: PermValue.Deny);
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                _ = voiceChannel.AddPermissionOverwriteAsync(parameter.Client.GetUserAsync(userId).Result, newPermissionOverride);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C134", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C134", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempblock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempLock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to block user from temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C135", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }

        public static async Task TempUnBlock(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "user").Result.IUser;

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            var userId = user.Id;

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter, nameof(TempUnBlock)).Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter, nameof(TempUnBlock)).Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter, nameof(TempUnBlock)).Result ||
                Bobii.CheckDatas.CheckIfUserInGuild(parameter, userId, nameof(TempUnBlock)).Result)
            {
                return;
            }

            try
            {
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                await voiceChannel.RemovePermissionOverwriteAsync(parameter.Client.GetUserAsync(userId).Result);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C136", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C136", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunblock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TempUnBlock), parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unblock user from temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("137", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            }
        }
        #endregion
    }
}
