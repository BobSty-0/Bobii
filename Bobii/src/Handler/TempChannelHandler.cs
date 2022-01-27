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

        public static async Task VoiceChannelActions(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice, DiscordSocketClient client, List<SocketUser> cooldownList)
        {
            SocketGuild guild;
            if (newVoice.VoiceChannel != null)
            {
                guild = newVoice.VoiceChannel.Guild;
                if (TempChannel.EntityFramework.TempChannelsHelper.DoesTempChannelExist(newVoice.VoiceChannel.Id).Result)
                {
                    var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(newVoice.VoiceChannel.Id).Result;
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
            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild);
            var tempchannelIDs = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList(guild.Id).Result;

            if (oldVoice.VoiceChannel != null)
            {
                if (tempchannelIDs.Count > 0)
                {
                    await TempChannel.Helper.CheckAndDeleteEmptyVoiceChannels(client, guild, tempchannelIDs, user);
                    if (newVoice.VoiceChannel == null)
                    {
                        return;
                    }
                }
            }

            if (newVoice.VoiceChannel != null)
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var createTempChannel = createTempChannels.Result.Where(ch => ch.createchannelid == newVoice.VoiceChannel.Id).FirstOrDefault();
                if (createTempChannel != null)
                {
                    if (!cooldownList.Contains(user))
                    {
                        await TempChannel.Helper.CreateAndConnectToVoiceChannel(user, newVoice, createTempChannel.tempchannelname, createTempChannel.channelsize, createTempChannel.textchannel.Value, client);
                    }
                    else
                    {
                        var guildUser = guild.GetUser(user.Id);
                        await guildUser.ModifyAsync(x => x.Channel = null);
                    }
                }
                stopWatch.Stop();
                Console.WriteLine($"Create and connect time: {stopWatch.ElapsedMilliseconds}");
            }
            else
            {
                return;
            }
        }
        #endregion
    }
}
