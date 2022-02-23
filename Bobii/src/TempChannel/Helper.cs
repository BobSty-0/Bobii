using Bobii.src.Entities;
using Bobii.src.EntityFramework.Entities;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class Helper
    {
        #region Tasks
        public static async Task<bool> ConnectBackToDelayedChannel(DiscordSocketClient client, SocketGuildUser user)
        {
            try
            {
                var tempChannels = EntityFramework.TempChannelsHelper.GetTempChannelList().Result.Where(t => t.channelownerid == user.Id);
                if (tempChannels.Count() == 0)
                {
                    return false;
                }

                foreach (var tempChannel in tempChannels)
                {
                    var voiceChannel = (SocketVoiceChannel)client.GetChannel(tempChannel.channelid);
                    if (voiceChannel == null)
                    {
                        continue;
                    }

                    await user.ModifyAsync(t => t.Channel = voiceChannel);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, nameof(ConnectBackToDelayedChannel),
                        new Entities.SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                        message: $"User successfully moved back into hes channel");
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, nameof(ConnectBackToDelayedChannel),
                    new Entities.SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                    message: $"Could not move user back into hes channel", exceptionMessage: ex.Message);
                return false;
            }
        }
        public static async Task GiveOwnerIfUserCountZero(SlashCommandParameter parameter)
        {
            try
            {
                var tempChannelId = parameter.GuildUser.VoiceState.Value.VoiceChannel.Id;
                if (parameter.GuildUser.VoiceState.Value.VoiceChannel.Users.Count == 0)
                {
                    var ownerId = EntityFramework.TempChannelsHelper.GetOwnerID(tempChannelId).Result;
                    if (ownerId != parameter.GuildUser.Id)
                    {
                        var tempChannel = EntityFramework.TempChannelsHelper.GetTempChannel(tempChannelId).Result;
                        if (tempChannel == null)
                        {
                            return;
                        }
                        await EntityFramework.TempChannelsHelper.ChangeOwner(tempChannelId, parameter.GuildUser.Id);
                        await GiveManageChannelRightsToUserVc(parameter.GuildUser, null, parameter.GuildUser.VoiceChannel);

                        if (tempChannel.textchannelid != 0)
                        {
                            var textChannel = parameter.Client.Guilds
                                .SelectMany(g => g.Channels)
                                .FirstOrDefault(c => c.Id == tempChannel.textchannelid);
                            await GiveManageChannelRightsToUserTc(parameter.GuildUser, null, textChannel as SocketTextChannel);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //nothing
            }
        }

        public static async Task GiveOwnerIfOwnerIDZero(Entities.SlashCommandParameter parameter)
        {
            try
            {
                var tempChannelId = parameter.GuildUser.VoiceState.Value.VoiceChannel.Id;
                var ownerId = EntityFramework.TempChannelsHelper.GetOwnerID(tempChannelId).Result;
                if (ownerId == 0)
                {
                    var tempChannel = EntityFramework.TempChannelsHelper.GetTempChannel(tempChannelId).Result;
                    if (tempChannel == null)
                    {
                        return;
                    }
                    await EntityFramework.TempChannelsHelper.ChangeOwner(tempChannelId, parameter.GuildUser.Id);
                    await GiveManageChannelRightsToUserVc(parameter.GuildUser, null, parameter.GuildUser.VoiceChannel);

                    if (tempChannel.textchannelid != 0)
                    {
                        var textChannel = parameter.Client.Guilds
                            .SelectMany(g => g.Channels)
                            .FirstOrDefault(c => c.Id == tempChannel.textchannelid);
                        await GiveManageChannelRightsToUserTc(parameter.GuildUser, null, textChannel as SocketTextChannel);
                    }
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

        public static async Task GiveManageChannelRightsToUserTc(SocketUser user, RestTextChannel restTextChannel, SocketTextChannel socketTextChannel)
        {
            if (restTextChannel != null)
            {
                await restTextChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(manageChannel: PermValue.Allow, viewChannel: PermValue.Allow));
            }
            else
            {
                await socketTextChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(manageChannel: PermValue.Allow, viewChannel: PermValue.Allow));
            }

        }

        public static async Task GiveViewChannelRightsToUserTc(SocketUser user, RestTextChannel restTextChannel, SocketTextChannel socketTextChannel)
        {
            if (restTextChannel != null)
            {
                await restTextChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(viewChannel: PermValue.Allow));
            }
            else
            {
                await socketTextChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions()
                    .Modify(viewChannel: PermValue.Allow));
            }
        }

        public static async Task RemoveViewChannelRightsFromUser(SocketUser user, SocketTextChannel textChannel)
        {
            await textChannel.RemovePermissionOverwriteAsync(user);
        }

        public static async Task RemoveManageChannelRightsToUserVc(SocketUser user, SocketVoiceChannel voiceChannel)
        {
            await voiceChannel.RemovePermissionOverwriteAsync(user);
        }

        public static async Task RemoveManageChannelRightsToUserTc(SocketUser user, SocketTextChannel textChannel)
        {
            await textChannel.RemovePermissionOverwriteAsync(user);
        }

        public static async Task<string> GetVoiceChannelName(createtempchannels createTempChannel, SocketUser user)
        {
            var tempChannelName = createTempChannel.tempchannelname;
            switch (tempChannelName)
            {
                case var s when tempChannelName.Contains("{count}"):
                    tempChannelName = tempChannelName.Replace("{count}",
                        (EntityFramework.TempChannelsHelper.GetCount(createTempChannel.createchannelid).Result).ToString());
                    break;
                case var s when tempChannelName.Contains("{activity}"):
                    var activityString = "Chilling";
                    foreach (var activity in user.Activities)
                    {
                        if (activity.Type == ActivityType.Playing || activity.Type == ActivityType.Listening || activity.Type == ActivityType.Watching)
                        {
                            activityString = activity.Name;
                        }

                        if (activity.Type == ActivityType.Streaming)
                        {
                            activityString = "Streaming";
                        }
                    }
                    tempChannelName = tempChannelName.Replace("{activity}", activityString);
                    break;
                case var s when tempChannelName.Contains("{username}"):
                    tempChannelName = tempChannelName.Replace("{username}", user.Username);
                    break;
            }
            await Task.CompletedTask;
            return tempChannelName;
        }

        public static async Task CreateAndConnectToVoiceChannel(SocketUser user, createtempchannels createTempChannel, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            var category = newVoice.VoiceChannel.Category;
            string channelName = GetVoiceChannelName(createTempChannel, user).Result;


            var tempChannel = Helper.CreateTextAndVoiceChannel(user, channelName, newVoice, createTempChannel).Result;
            await TempChannel.Helper.ConnectToVoice(tempChannel, user as IGuildUser);
        }

        public static async Task<RestVoiceChannel> CreateTextAndVoiceChannel(SocketUser user, string channelName, SocketVoiceState newVoice, createtempchannels createTempChannel)
        {
            try
            {
                var category = newVoice.VoiceChannel.Category;
                var tempChannel = CreateVoiceChannel(user as SocketGuildUser, category.Id.ToString(), channelName, createTempChannel.channelsize, newVoice).Result;
                if (createTempChannel.textchannel.Value)
                {
                    var textChannelRestChannel = CreateTextChannel(user as SocketGuildUser, newVoice, channelName, category.Id.ToString()).Result;
                    _ = EntityFramework.TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id, textChannelRestChannel.Id);
                }
                else
                {
                    _ = EntityFramework.TempChannelsHelper.AddTC(newVoice.VoiceChannel.Guild.Id, tempChannel.Id, newVoice.VoiceChannel.Id, user.Id, 0);
                }
                return tempChannel;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, nameof(CreateTextAndVoiceChannel), createChannelID: createTempChannel.createchannelid);
                return null;
            }
        }

        public static async Task ConnectToVoice(RestVoiceChannel voiceChannel, IGuildUser user)
        {
            if (voiceChannel == null)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, "ConnectToVoice",
                    new Entities.SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user }, message: $"{ user} ({ user.Id}) could not be connected");
                return;
            }
            await user.ModifyAsync(x => x.Channel = voiceChannel);
            await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "ConnectToVoice",
                  new Entities.SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                  message: $"{user} ({user.Id}) was successfully connected to {voiceChannel}", tempChannelID: voiceChannel.Id);
        }

        public static async Task CheckForTempChannelCorps(SlashCommandParameter parameter)
        {
            var tempChannels = EntityFramework.TempChannelsHelper.GetTempChannelList().Result;
            try
            {
                foreach (var tempChannel in tempChannels)
                {
                    var channel = (SocketVoiceChannel)parameter.Client.GetChannel(tempChannel.channelid);

                    if (channel == null)
                    {
                        await EntityFramework.TempChannelsHelper.RemoveTC(0, tempChannel.channelid);
                        await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, nameof(CheckForTempChannelCorps),
                            parameter, message: "Corps detected!", tempChannelID: tempChannel.channelid);
                    }
                }
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, nameof(CheckForTempChannelCorps),
                    parameter, message: "Corps detected!", exceptionMessage: ex.Message);
            }
        }

        public static async Task CheckAndDeleteEmptyVoiceChannels(DiscordSocketClient client, SocketGuild guild, List<tempchannels> tempchannelIDs, SocketUser user, bool deleteChannel = false)
        {
            var voiceChannelName = "";
            var socketGuildUser = guild.GetUser(user.Id);
            try
            {
                foreach (var tempChannel in tempchannelIDs)
                {
                    var refreshedGuild = client.GetGuild(guild.Id);
                    var voiceChannel = refreshedGuild.VoiceChannels.SingleOrDefault(t => t.Id == tempChannel.channelid);

                    if (voiceChannel == null)
                    {
                        continue;
                    }
                    if (!deleteChannel)
                    {
                        if (tempChannel.deletedate != null && (tempChannel.deletedate.Value - DateTime.Now).TotalMinutes >= 0)
                        {
                            continue;
                        }
                    }

                    voiceChannelName = voiceChannel.Name;

                    if (voiceChannel.Users.Count == 0)
                    {
                        await voiceChannel.DeleteAsync();
                        
                        await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "CheckAndDeleteEmptyVoiceChannels",
                              new Entities.SlashCommandParameter() { Guild = guild, GuildUser = socketGuildUser },
                              message: $"Channel successfully deleted", tempChannelID: tempChannel.channelid);

                        var tempChannelEF = EntityFramework.TempChannelsHelper.GetTempChannel(tempChannel.channelid).Result;

                        if (tempChannelEF.textchannelid != 0)
                        {
                            var textChannel = refreshedGuild.Channels.SingleOrDefault(t => t.Id == tempChannelEF.textchannelid);

                            if (textChannel != null)
                            {
                                await textChannel.DeleteAsync();
                                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "DeleteTextChannel",
                                      new Entities.SlashCommandParameter() { Guild = guild, GuildUser = (SocketGuildUser)user },
                                      message: $"Text channel successfully deleted", tempChannelID: tempChannel.channelid);
                            }
                        }

                        await EntityFramework.TempChannelsHelper.RemoveTC(guild.Id, tempChannel.channelid);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Access"))
                {
                    var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guild.Id).Result;
                    await user.SendMessageAsync(string.Format(Bobii.Helper.GetContent("C097", language).Result, user.Username, voiceChannelName));
                }
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, "CheckAndDeleteEmptyVoiceChannels",
                    new Entities.SlashCommandParameter() { Guild = guild, GuildUser = socketGuildUser },
                    message: $"Voicechannel could not be deleted, {user} has got a DM if it was missing access", exceptionMessage: ex.Message);
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
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "ConnectToVoice",
                          new Entities.SlashCommandParameter() { Guild = (SocketGuild)user.Guild, GuildUser = (SocketGuildUser)user },
                          message: $"{user} ({user.Id}) was successfully connected to {channel.Name}", tempChannelID: channel.Id);
                    return true;
                }
            }

            return false;
        }

        public static async Task<RestTextChannel> CreateTextChannel(SocketGuildUser user, SocketVoiceState newVoice, string name, string catergoryId)
        {
            try
            {
                List<Overwrite> permissions = new List<Overwrite>();
                var everyoneRole = user.Guild.Roles.First(r => r.Name == "@everyone");

                SocketRole bobiiRole = null;
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    bobiiRole = user.Guild.Roles.Where(role => role.Name == "BobiiDev").First();
                }
                else
                {
                    bobiiRole = user.Guild.Roles.Where(role => role.Name == "Bobii").First();
                }

                permissions.Add(new Overwrite(everyoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
                permissions.Add(new Overwrite(bobiiRole.Id, PermissionTarget.Role, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, moveMembers: PermValue.Allow)));
                //Create channel with permissions in the target category
                var channel = user.Guild.CreateTextChannelAsync(name, prop =>
                {
                    prop.CategoryId = ulong.Parse(catergoryId);
                    prop.PermissionOverwrites = permissions;
                });

                await GiveManageChannelRightsToUserTc(user, channel.Result, null);
                await GiveViewChannelRightsToUserTc(user, channel.Result, null);

                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, nameof(CreateTextChannel),
                    new Entities.SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"{user} created new text channel {channel.Result}", tempChannelID: channel.Result.Id);
                return channel.Result;
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, nameof(CreateTextChannel),
                    new Entities.SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"Text channel could not be created", exceptionMessage: ex.Message);
                return null;
                throw;
            }

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

                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", false, "CreateVoiceChannel",
                    new Entities.SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"{user} created new voice channel {channel.Result}", tempChannelID: channel.Result.Id);
                return channel.Result;
            }
            catch (Exception ex)
            {
                var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(user.Guild.Id).Result;
                if (ex.Message.Contains("Missing Permission"))
                {
                    await user.SendMessageAsync(String.Format(Bobii.Helper.GetContent("C098", language).Result, user.Username));
                }

                if (ex.Message.Contains("Object reference not set to an instance of an object"))
                {
                    await user.SendMessageAsync(String.Format(Bobii.Helper.GetContent("C099", language).Result, user.Username));
                }

                await Handler.HandlingService._bobiiHelper.WriteToConsol("TempVoiceC", true, "CreateVoiceChannel",
                    new Entities.SlashCommandParameter() { Guild = user.Guild, GuildUser = user },
                    message: $"Voicechannel could not be created, {user} has got a DM if it was missing permissions or null ref", exceptionMessage: ex.Message);
                return null;
            }
        }

        public static Embed CreateVoiceChatInfoEmbed(Entities.SlashCommandParameter parameter)
        {
            var config = Program.GetConfig();
            StringBuilder sb = new StringBuilder();
            var createTempChannelList = EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(parameter.Guild).Result;
            string header = null;
            if (createTempChannelList.Count == 0)
            {
                header = Bobii.Helper.GetCaption("C100", parameter.Language).Result;
                sb.AppendLine(Bobii.Helper.GetContent("C100", parameter.Language).Result);
            }
            else
            {
                header = Bobii.Helper.GetCaption("C101", parameter.Language).Result;
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

                if (createTempChannel.channelsize != null)
                {
                    sb.AppendLine($"TempChannelSize: **{createTempChannel.channelsize}**");
                }

                if (createTempChannel.textchannel.Value)
                {
                    sb.AppendLine("Text channel: **On**");
                }
            }

            return Bobii.Helper.CreateEmbed(parameter.Interaction, sb.ToString(), header).Result;
        }

        //Double Code -> Find solution one day!
        public static async Task<string> HelpTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId)
        {
            await Task.CompletedTask;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            return Bobii.Helper.CreateInfoPart(commandList, language,
                Bobii.Helper.GetContent("C102", language).Result +
                Bobii.Helper.GetContent("C103", language).Result, "tc").Result;
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
                sb.AppendLine(Bobii.Helper.GetContent("C104", language).Result);
                sb.AppendLine(Bobii.Helper.GetContent("C105", language).Result);
            }
            return Bobii.Helper.CreateInfoPart(commandList, language, sb.ToString(), "temp").Result;
        }

        public static async Task TansferOwnerShip(SocketVoiceChannel channel, DiscordSocketClient client)
        {
            if (channel.Users.Where(u => u.IsBot == false).Count() == 0)
            {
                if (channel.Users.Count != 0)
                {
                    await EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, 0);
                }
                return;
            }
            var luckyNewOwner = channel.Users.Where(u => u.IsBot == false).First();
            await GiveManageChannelRightsToUserVc(luckyNewOwner, null, channel);

            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(channel.Id).Result;
            if (tempChannel.textchannelid != 0)
            {
                var textChannel = client.Guilds
                    .SelectMany(g => g.Channels)
                    .SingleOrDefault(c => c.Id == tempChannel.textchannelid);
                if (textChannel != null)
                {
                    await GiveManageChannelRightsToUserTc(luckyNewOwner, null, textChannel as SocketTextChannel);
                }
            }
            await EntityFramework.TempChannelsHelper.ChangeOwner(channel.Id, luckyNewOwner.Id);
        }
        #endregion
    }
}