using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class TempChannelHandler
    {
        #region Handler

        public static async Task VoiceChannelActions(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice, DiscordSocketClient client, List<SocketUser> cooldownList, TempChannel.DelayOnDelete delayAndDelete)
        {
            SocketGuild guild;
            if (newVoice.VoiceChannel != null)
            {
                guild = newVoice.VoiceChannel.Guild;
                if (TempChannel.EntityFramework.TempChannelsHelper.DoesTempChannelExist(newVoice.VoiceChannel.Id).Result)
                {
                    var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(newVoice.VoiceChannel.Id).Result;
                    if (tempChannel.deletedate != null)
                    {
                        await delayAndDelete.StopDelay(tempChannel);
                        await TempChannel.EntityFramework.TempChannelsHelper.ChangeOwner(tempChannel.channelid, user.Id);

                        await TempChannel.Helper.GiveManageChannelRightsToUserVc((SocketGuildUser)user, null, ((SocketGuildUser)user).VoiceChannel);

                        if (tempChannel.textchannelid != 0)
                        {
                            var textChannel = client.Guilds
                                .SelectMany(g => g.Channels)
                                .FirstOrDefault(c => c.Id == tempChannel.textchannelid);
                            await TempChannel.Helper.GiveManageChannelRightsToUserTc((SocketGuildUser)user, null, textChannel as SocketTextChannel);
                        }
                    }

                    if (tempChannel.textchannelid != 0)
                    {
                        var textChannel = client.Guilds
                            .SelectMany(g => g.Channels)
                            .SingleOrDefault(c => c.Id == tempChannel.textchannelid);

                        if (textChannel != null)
                        {
                            await TempChannel.Helper.GiveViewChannelRightsToUserTc(user, null, textChannel as SocketTextChannel);
                        }
                    }
                }
            }
            else
            {
                guild = oldVoice.VoiceChannel.Guild;
                if (TempChannel.EntityFramework.TempChannelsHelper.DoesTempChannelExist(oldVoice.VoiceChannel.Id).Result)
                {
                    var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(oldVoice.VoiceChannel.Id).Result;
                    if (tempChannel.textchannelid != 0)
                    {
                        var textChannel = client.Guilds
                            .SelectMany(g => g.Channels)
                            .SingleOrDefault(c => c.Id == tempChannel.textchannelid);

                        if (textChannel != null)
                        {
                            await TempChannel.Helper.RemoveViewChannelRightsFromUser(user, textChannel as SocketTextChannel);
                        }
                    }
                }
                if (TempChannel.EntityFramework.TempChannelsHelper.DoesOwnerExist(user.Id).Result && TempChannel.EntityFramework.TempChannelsHelper.DoesTempChannelExist(oldVoice.VoiceChannel.Id).Result)
                {
                    var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(oldVoice.VoiceChannel.Id).Result;
                    if (tempChannel.textchannelid != 0)
                    {
                        var textChannel = client.Guilds
                            .SelectMany(g => g.Channels)
                            .SingleOrDefault(c => c.Id == tempChannel.textchannelid);

                        if (textChannel != null)
                        {
                            await TempChannel.Helper.RemoveManageChannelRightsToUserTc(user, textChannel as SocketTextChannel);
                        }
                    }
                    await TempChannel.Helper.RemoveManageChannelRightsToUserVc(user, oldVoice.VoiceChannel);
                    await TempChannel.Helper.TansferOwnerShip(oldVoice.VoiceChannel, client);
                }
            }
            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild).Result;
            var tempchannelIDs = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelListFromGuild(guild.Id).Result;

            if (oldVoice.VoiceChannel != null)
            {
                if (tempchannelIDs.Count > 0)
                {
                    var tempChannel = tempchannelIDs.FirstOrDefault(c => c.channelid == oldVoice.VoiceChannel.Id);
                    if (tempChannel != null)
                    {
                        var createTempChannel = createTempChannels.Where(ch => ch.createchannelid == tempChannel.createchannelid).FirstOrDefault();

                        if (createTempChannel.delay != null)
                        {
                            await delayAndDelete.StartDelay(tempChannel, createTempChannel, client, guild, user, tempchannelIDs);
                        }
                        else
                        {
                            await TempChannel.Helper.CheckAndDeleteEmptyVoiceChannels(client, guild, tempchannelIDs, user);
                            if (newVoice.VoiceChannel == null)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            if (newVoice.VoiceChannel != null)
            {
                var createTempChannel = createTempChannels.Where(ch => ch.createchannelid == newVoice.VoiceChannel.Id).FirstOrDefault();
                if (createTempChannel != null)
                {
                    if(createTempChannel.delay != 0 && TempChannel.Helper.ConnectBackToDelayedChannel(client, (SocketGuildUser)user).Result)
                    {
                        return;
                    }

                    if (!cooldownList.Contains(user))
                    {
                        await TempChannel.Helper.CreateAndConnectToVoiceChannel(user, createTempChannel, newVoice, client);
                    }
                    else
                    {
                        var guildUser = guild.GetUser(user.Id);
                        await guildUser.ModifyAsync(x => x.Channel = null);
                    }
                }
            }
            else
            {
                return;
            }
        }
        #endregion
    }
}
