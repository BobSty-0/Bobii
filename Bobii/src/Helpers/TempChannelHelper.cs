using Bobii.src.Bobii;
using Bobii.src.EntityFramework.Entities;
using Bobii.src.Enums;
using Bobii.src.Helper;
using Bobii.src.Models;
using Bobii.src.TempChannel;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Helper
{
    class TempChannelHelper
    {
        #region Tasks
        public static async Task HandleUserJoinedChannel(VoiceUpdatedParameter parameter)
        {
            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(parameter.NewSocketVoiceChannel.Id).Result;
            var createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(parameter.Guild).Result
                .SingleOrDefault(channel => channel.createchannelid == parameter.NewSocketVoiceChannel.Id);

            if (createTempChannel != null)
            {
                var existingTempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result.OrderByDescending(ch => ch.id).FirstOrDefault(c => c.channelownerid == parameter.SocketUser.Id && c.createchannelid == createTempChannel.createchannelid);
                if (existingTempChannel != null)
                {
                    var guildUser = parameter.Guild.GetUser(parameter.SocketUser.Id);
                    var tempVoice = (SocketVoiceChannel)parameter.Client.GetChannel(existingTempChannel.channelid);
                    if (tempVoice.ConnectedUsers.Count == 0)
                    {
                        await guildUser.ModifyAsync(c => c.Channel = tempVoice);
                        return;
                    }
                }
            }

            // Giving view rights in case that the temp-channel has a linked text-channel
            if (tempChannel != null)
            {
                if (tempChannel.deletedate != null)
                {
                    // Stopping the delay if another user joins the voice channel which has an delay
                    await parameter.DelayOnDelete.StopDelay(tempChannel);
                    return;
                }
            }

            // Checking if the joined channel is a create-temp-channel
            if (createTempChannel != null)
            {
                string channelName = string.Empty;
                var tempChannelName = string.Empty;

                if (TempChannel.EntityFramework.TempChannelUserConfig.TempChannelUserConfigExists(parameter.SocketUser.Id, createTempChannel.createchannelid).Result)
                {
                    var tempChannelConfig = TempChannel.EntityFramework.TempChannelUserConfig.GetTempChannelConfig(parameter.SocketUser.Id, createTempChannel.createchannelid).Result;
                    tempChannelName = tempChannelConfig.tempchannelname;
                    tempChannelName = GetVoiceChannelName(createTempChannel, parameter.SocketUser, tempChannelName, parameter.Client).Result;
                    await CreateAndConnectToVoiceChannel(parameter.SocketUser, createTempChannel, parameter.NewVoiceState, parameter.Client, tempChannelConfig.channelsize, tempChannelName);
                    return;
                }

                tempChannelName = createTempChannel.tempchannelname;
                tempChannelName = GetVoiceChannelName(createTempChannel, parameter.SocketUser, tempChannelName, parameter.Client).Result;
                await CreateAndConnectToVoiceChannel(parameter.SocketUser, createTempChannel, parameter.NewVoiceState, parameter.Client, createTempChannel.channelsize, tempChannelName);
            }
        }

        public static async Task HandleUserLeftChannel(VoiceUpdatedParameter parameter)
        {
            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(parameter.OldSocketVoiceChannel.Id).Result;

            createtempchannels createTempChannel;
            if (parameter.VoiceUpdated == VoiceUpdated.UserLeftAndJoinedChannel)
            {
                createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(parameter.Guild).Result
                    .SingleOrDefault(channel => channel.createchannelid == parameter.NewSocketVoiceChannel.Id);

                if (createTempChannel != null)
                {
                    var existingTempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result.OrderByDescending(ch => ch.id).FirstOrDefault(c => c.channelownerid == parameter.SocketUser.Id && c.createchannelid == createTempChannel.createchannelid);
                    if (existingTempChannel != null)
                    {
                        var guildUser = parameter.Guild.GetUser(parameter.SocketUser.Id);
                        var tempVoice = (SocketVoiceChannel)parameter.Client.GetChannel(existingTempChannel.channelid);
                        if (tempVoice != null && tempVoice.Id == parameter.OldSocketVoiceChannel.Id && tempVoice.ConnectedUsers.Count == 0)
                        {
                            return;
                        }
                    }
                }
            }

            if (tempChannel == null)
            {
                return;
            }

            // Removing view rights of the text-channel if the temp-chanenl has a linked text-channel
            createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelList().Result.FirstOrDefault(c => c.createchannelid == tempChannel.createchannelid);
            if (createTempChannel != null && createTempChannel.delay != null && createTempChannel.delay != 0 && parameter.OldSocketVoiceChannel.ConnectedUsers.Count == 0)
            {
                // We just add an delay if the createTempChannel has an delay
                await parameter.DelayOnDelete.StartDelay(tempChannel, createTempChannel, parameter);
                return;
            }

            if (parameter.OldSocketVoiceChannel.ConnectedUsers.Count() == 0)
            {
                await TempChannelHelper.DeleteTempChannel(parameter, tempChannel);
                return;
            }

            // If the user was the owner of the temp-channel which he left, then the owner ship will be transfered to a new random owner
            if (tempChannel.channelownerid == parameter.SocketUser.Id)
            {
                await RemoveManageChannelRightsToUserVc(parameter.SocketUser, parameter.OldSocketVoiceChannel);
                await TansferOwnerShip(parameter.OldSocketVoiceChannel, parameter.Client);
            }
        }

        public static async Task<VoiceUpdatedParameter> GetVoiceUpdatedParameter(SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState, SocketUser user, DiscordSocketClient client, DelayOnDelete delayOnDeleteClass)
        {
            var parameter = new VoiceUpdatedParameter();
            parameter.VoiceUpdated = GetVoiceUpdatedEnum(oldVoiceState, newVoiceState).Result;
            parameter.Client = client;
            parameter.SocketUser = user;
            parameter.DelayOnDelete = delayOnDeleteClass;
            if (parameter.VoiceUpdated == VoiceUpdated.UserLeftAndJoinedChannel)
            {
                parameter.NewSocketVoiceChannel = newVoiceState.VoiceChannel;
                parameter.OldSocketVoiceChannel = oldVoiceState.VoiceChannel;
                parameter.OldVoiceState = oldVoiceState;
                parameter.NewVoiceState = newVoiceState;
                parameter.Guild = parameter.NewSocketVoiceChannel.Guild;

            }

            if (parameter.VoiceUpdated == VoiceUpdated.UserJoinedAChannel)
            {
                parameter.NewSocketVoiceChannel = newVoiceState.VoiceChannel;
                parameter.NewVoiceState = newVoiceState;
                parameter.Guild = parameter.NewSocketVoiceChannel.Guild;
            }
            else
            {
                parameter.OldSocketVoiceChannel = oldVoiceState.VoiceChannel;
                parameter.OldVoiceState = oldVoiceState;
                parameter.Guild = parameter.OldSocketVoiceChannel.Guild;
            }
            await Task.CompletedTask;
            return parameter;
        }

        public static async Task<VoiceUpdated> GetVoiceUpdatedEnum(SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {
            if (oldVoiceState.VoiceChannel == null && newVoiceState.VoiceChannel == null)
            {
                return VoiceUpdated.ChannelDestroyed;
            }
            if (oldVoiceState.VoiceChannel != null && newVoiceState.VoiceChannel != null)
            {
                return VoiceUpdated.UserLeftAndJoinedChannel;
            }

            if (oldVoiceState.VoiceChannel != null)
            {
                return VoiceUpdated.UserLeftAChannel;
            }
            if (newVoiceState.VoiceChannel != null)
            {
                return VoiceUpdated.UserJoinedAChannel;
            }
            // Will never happen
            return VoiceUpdated.ChannelDestroyed;
        }

        public static async Task<bool> ConnectBackToDelayedChannel(DiscordSocketClient client, SocketGuildUser user, createtempchannels createTempChannel)
        {
            try
            {
                var tempChannels = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result.Where(t => t.channelownerid == user.Id).ToList();
                if (tempChannels.Count() == 0)
                {
                    return false;
                }

                foreach (var tempChannel in tempChannels)
                {
                    if (tempChannel.createchannelid != createTempChannel.createchannelid)
                    {
                        continue;
                    }
                    var voiceChannel = (SocketVoiceChannel)client.GetChannelAsync(tempChannel.channelid).Result;
                    if (voiceChannel == null)
                    {
                        continue;
                    }

                    await user.ModifyAsync(t => t.Channel = voiceChannel);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, nameof(ConnectBackToDelayedChannel),
                        new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                        message: $"User successfully moved back into hes channel");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, nameof(ConnectBackToDelayedChannel),
                    new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                    message: $"Could not move user back into hes channel", exceptionMessage: ex.Message);
                return false;
            }
        }

        public static async Task GiveOwnerIfOwnerIDZero(SlashCommandParameter parameter)
        {
            try
            {
                var tempChannelId = parameter.GuildUser.VoiceState.Value.VoiceChannel.Id;
                var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(tempChannelId).Result;
                if (tempChannel == null)
                {
                    return;
                }

                var ownerId = TempChannel.EntityFramework.TempChannelsHelper.GetOwnerID(tempChannelId).Result;
                if (ownerId == 0)
                {
                    await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(tempChannelId, parameter.GuildUser.Id);
                    await GiveManageChannelRightsToUserVc(parameter.GuildUser, null, parameter.GuildUser.VoiceChannel);
                }
            }
            catch (Exception)
            {
                //nothing
            }
        }

        public static async Task GiveManageChannelRightsToUserVc(SocketUser user, RestVoiceChannel restVoiceChannel, SocketVoiceChannel socketVoiceChannel)
        {
            if (restVoiceChannel != null)
            {
                await restVoiceChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(manageChannel: PermValue.Allow));
            }
            else
            {
                await socketVoiceChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(manageChannel: PermValue.Allow));
            }
        }

        public static async Task RemoveManageChannelRightsToUserVc(SocketUser user, SocketVoiceChannel voiceChannel)
        {
            await voiceChannel.RemovePermissionOverwriteAsync(user);
        }

        public static async Task<string> GetVoiceChannelName(createtempchannels createTempChannel, SocketUser user, string tempChannelName, DiscordSocketClient client)
        {
            switch (tempChannelName)
            {
                case var s when tempChannelName.Contains("{count}"):
                    tempChannelName = tempChannelName.Replace("{count}",
                        (TempChannel.EntityFramework.TempChannelsHelper.GetCount(createTempChannel.createchannelid).Result).ToString());
                    break;
                case var s when tempChannelName.Contains("{username}"):
                    tempChannelName = tempChannelName.Replace("{username}", user.GlobalName);
                    break;
                case var s when tempChannelName.Contains("{nickname}"):
                    var guildUser = client.GetGuild(createTempChannel.guildid)?.GetUser(user.Id);

                    if (guildUser == null || guildUser.Nickname == null)
                    {
                        tempChannelName = tempChannelName.Replace("{nickname}", user.GlobalName);
                    }

                    tempChannelName = tempChannelName.Replace("{nickname}", guildUser.Nickname);
                    break;
            }
            await Task.CompletedTask;
            return tempChannelName;
        }

        public static async Task CreateAndConnectToVoiceChannel(
            SocketUser user,
            createtempchannels createTempChannel,
            SocketVoiceState newVoice,
            DiscordSocketClient client,
            int? channelSize,
            string channelName)
        {
            var tempChannel = CreateVoiceChannel(user, channelName, newVoice, createTempChannel, channelSize).Result;
            await ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketUser user, string channelName, SocketVoiceState newVoice, createtempchannels createTempChannel, int? channelSize)
        {
            try
            {
                var category = newVoice.VoiceChannel.Category;
                var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), channelName, channelSize, newVoice).Result;

                _ = TempChannel.EntityFramework.TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id);
                return tempChannel;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, nameof(CreateVoiceChannel), createChannelID: createTempChannel.createchannelid);
                return null;
            }
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            if (voiceChannel == null)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, "ConnectToVoice",
                    new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user }, message: $"{ user} ({ user.Id}) could not be connected");
                return;
            }
            await user.ModifyAsync(x => x.Channel = voiceChannel);
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "ConnectToVoice",
                  new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                  message: $"{user} ({user.Id}) was successfully connected to {voiceChannel}", tempChannelID: voiceChannel.Id);
        }

        public static async Task CheckForTempChannelCorps(SlashCommandParameter parameter)
        {
            var tempChannels = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result;
            try
            {
                foreach (var tempChannel in tempChannels)
                {
                    var channel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.channelid);

                    if (channel == null)
                    {
                        await TempChannel.EntityFramework.TempChannelsHelper.RemoveTC(0, tempChannel.channelid);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, nameof(CheckForTempChannelCorps),
                            parameter, message: "Corps detected!", tempChannelID: tempChannel.channelid);
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, nameof(CheckForTempChannelCorps),
                    parameter, message: "Corps detected!", exceptionMessage: ex.Message);
            }
        }

        public static async Task DeleteTempChannel(VoiceUpdatedParameter parameter, tempchannels tempChannel)
        {
            var socketGuildUser = parameter.Guild.GetUser(parameter.SocketUser.Id);
            await parameter.OldSocketVoiceChannel.DeleteAsync();

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "CheckAndDeleteEmptyVoiceChannels",
                  new SlashCommandParameter() { Guild = parameter.Guild, GuildUser = socketGuildUser },
                  message: $"Channel successfully deleted", tempChannelID: tempChannel.channelid);

            if (TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(tempChannel.channelid).Result != null)
            {
                await TempChannel.EntityFramework.TempChannelsHelper.RemoveTC(parameter.Guild.Id, tempChannel.channelid);
            }
        }

        public static async Task CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client)
        {
            var voiceChannelName = "";
            var guild = client.GetGuild(GeneralHelper.GetConfigKeyValue(ConfigKeys.MainGuildID).ToUlong());
            var socketGuildUser = guild.GetUser(GeneralHelper.GetConfigKeyValue(ConfigKeys.MainGuildID).ToUlong());
            var tempChannelIDs = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList().Result;
            try
            {
                foreach (var tempChannel in tempChannelIDs)
                {
                    if (tempChannel.deletedate != null)
                    {
                        continue;
                    }

                    var voiceChannel = (SocketVoiceChannel)client.GetChannel(tempChannel.channelid);
                    if (voiceChannel == null)
                    {
                        continue;
                    }

                    voiceChannelName = voiceChannel.Name;

                    if (voiceChannel.ConnectedUsers.Count == 0)
                    {
                        await voiceChannel.DeleteAsync();

                        await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "CheckAndDeleteEmptyVoiceChannels",
                              new SlashCommandParameter() { Guild = guild, GuildUser = socketGuildUser },
                              message: $"Channel successfully deleted", tempChannelID: tempChannel.channelid);

                        var tempChannelEF = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(tempChannel.channelid).Result;

                        await TempChannel.EntityFramework.TempChannelsHelper.RemoveTC(guild.Id, tempChannel.channelid);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Access"))
                {
                    var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guild.Id).Result;
                    await socketGuildUser.SendMessageAsync(string.Format(GeneralHelper.GetContent("C097", language).Result, socketGuildUser.Username, voiceChannelName));
                }
                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, "CheckAndDeleteEmptyVoiceChannels",
                    new SlashCommandParameter() { Guild = guild, GuildUser = socketGuildUser },
                    message: $"Voicechannel could not be deleted, {socketGuildUser} has got a DM if it was missing access", exceptionMessage: ex.Message);
            }
        }

        public static async Task<bool> CheckIfChannelWithNameExists(SocketGuildUser user, string catergoryId, string name, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            var guild = user.Guild;
            var categoryChannel = guild.GetCategoryChannel(ulong.Parse(catergoryId));

            foreach (var channel in categoryChannel.Channels)
            {
                if (channel.Name == name)
                {
                    await user.ModifyAsync(x => x.Channel = (SocketVoiceChannel)channel);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "ConnectToVoice",
                          new SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                          message: $"{user} ({user.Id}) was successfully connected to {channel.Name}", tempChannelID: channel.Id);
                    return true;
                }
            }

            return false;
        }

        public static async Task<RestVoiceChannel> CreateVoiceChannel(SocketGuildUser user, string catergoryId, string name, int? channelSize, SocketVoiceState newVoice)
        {
            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                //Permissions for each role
                foreach (var role in user.Guild.Roles)
                {
                    var permissionOverride = newVoice.VoiceChannel.GetPermissionOverwrite(role);
                    if (permissionOverride != null)
                    {
                        permissions.Add(new Overwrite(role.Id, PermissionTarget.Role, permissionOverride.Value));
                    }
                }

                SocketRole bobiiRole = null;
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    bobiiRole = user.Guild.Roles.Where(role => role.Name == "BobiiDev").First();
                }
                else
                {
                    bobiiRole = user.Guild.Roles.Where(role => role.Name == "Bobii").First();
                }

                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));

                //Create channel with permissions in the target category
                var channel = user.Guild.CreateVoiceChannelAsync(name, prop =>
                {
                    prop.CategoryId = ulong.Parse(catergoryId);
                    prop.PermissionOverwrites = permissions;
                    prop.UserLimit = channelSize;
                });

                await GiveManageChannelRightsToUserVc(user, channel.Result, null);

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, false, "CreateVoiceChannel",
                    new SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"{user} created new voice channel {channel.Result}", tempChannelID: channel.Result.Id);
                return channel.Result;
            }
            catch (Exception ex)
            {
                var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(user.Guild.Id).Result;
                if (ex.Message.Contains("Missing Permission"))
                {
                    await user.SendMessageAsync(String.Format(GeneralHelper.GetContent("C098", language).Result, user.Username));
                }

                if (ex.Message.Contains("Object reference not set to an instance of an object"))
                {
                    await user.SendMessageAsync(String.Format(GeneralHelper.GetContent("C099", language).Result, user.Username));
                }

                await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.TempVoiceC, true, "CreateVoiceChannel",
                    new SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"Voicechannel could not be created, {user} has got a DM if it was missing permissions or null ref", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static Embed CreateVoiceChatInfoEmbed(SlashCommandParameter parameter)
        {
            StringBuilder sb = new StringBuilder();
            var createTempChannelList = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(parameter.Guild).Result;
            string header = null;
            if (createTempChannelList.Count == 0)
            {
                header = GeneralHelper.GetCaption("C100", parameter.Language).Result;
                sb.AppendLine(GeneralHelper.GetContent("C100", parameter.Language).Result);
            }
            else
            {
                header = GeneralHelper.GetCaption("C101", parameter.Language).Result;
            }

            foreach (var createTempChannel in createTempChannelList)
            {
                var channelId = createTempChannel.createchannelid;
                var voiceChannel = parameter.Client.Guilds
                                   .SelectMany(g => g.Channels)
                                   .SingleOrDefault(c => c.Id == channelId);
                if (voiceChannel == null)
                {
                    continue;
                }

                sb.AppendLine("");
                sb.AppendLine($"<#{channelId}>");
                sb.AppendLine($"Id: **{channelId}**");
                sb.AppendLine($"TempChannelName: **{createTempChannel.tempchannelname}**");

                if (createTempChannel.channelsize != null && createTempChannel.channelsize != 0)
                {
                    sb.AppendLine($"TempChannelSize: **{createTempChannel.channelsize}**");
                }

                if (createTempChannel.delay.HasValue && createTempChannel.delay != 0)
                {
                    sb.AppendLine($"Delay: **{createTempChannel.delay}**");
                }
            }

            return GeneralHelper.CreateEmbed(parameter.Interaction, sb.ToString(), header).Result;
        }

        //Double Code -> Find solution one day!
        public static async Task<string> HelpTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            return GeneralHelper.CreateInfoPart(
                commandList,
                language,
                GeneralHelper.GetContent("C102", language).Result + GeneralHelper.GetContent("C103", language).Result,
                "createtempchannel", 
                guildId).Result;
        }

        public static async Task<string> HelpEditTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId, bool withoutHint = false)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            var sb = new StringBuilder();
            if (!withoutHint)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine(GeneralHelper.GetContent("C104", language).Result);
                sb.AppendLine(GeneralHelper.GetContent("C105", language).Result);
                sb.AppendLine();
                sb.AppendLine(GeneralHelper.GetContent("C187", language).Result);
            }
            return GeneralHelper.CreateInfoPart(commandList, language, sb.ToString(), "temp", guildId, !withoutHint).Result;
        }

        public static async Task TansferOwnerShip(SocketVoiceChannel channel, DiscordSocketClient client)
        {
            if (channel.ConnectedUsers.Where(u => u.IsBot == false).Count() == 0)
            {
                if (channel.ConnectedUsers.Count != 0)
                {
                    await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, 0);
                }
                return;
            }
            var luckyNewOwner = channel.ConnectedUsers.Where(u => u.IsBot == false).First();
            await GiveManageChannelRightsToUserVc(luckyNewOwner, null, channel);

            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(channel.Id).Result;
            await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, luckyNewOwner.Id);
        }
        #endregion
    }
}