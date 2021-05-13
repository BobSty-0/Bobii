using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.CommandsNext;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using DSharpPlus;
using DSharpPlus.VoiceNext;

namespace DiscordBotBobSty.Commands
{
    class MusicCommands : BaseCommandModule
    {
        public async Task Join(CommandContext ctx, DiscordChannel channel)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            await node.ConnectAsync(channel);
        }

        [Command("leave")]
        [Description("'leave\n Disconnects the bot from your voice channel")]
        public async Task Leave(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            DiscordChannel channel = ctx.Member.VoiceState?.Channel;
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync();
        }

        [Command("play")]
        [Description("'play <name of the yt vid>\nJoins your voicechannel and plays the song you wrote")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await ctx.Message.DeleteAsync();
            await Join(ctx, ctx.Member.VoiceState?.Channel);
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for: '{search}'");
                return;
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await ctx.RespondAsync($"Now playing {track.Uri}!");
        }

        [Command("pause")]
        [Description("'pause \nPauses the song which it is playing rn")]
        public async Task Pause(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
        }
    }
}
