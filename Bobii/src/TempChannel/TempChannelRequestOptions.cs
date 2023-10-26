using Bobii.src.Helper;
using Bobii.src.Models;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    public class RateLimitHandler
    {
        private SlashCommandParameter _parameter;

        public RateLimitHandler( SlashCommandParameter parameter)
        {
            _parameter = parameter;
        }

        public Task MyRatelimitCallback(IRateLimitInfo arg)
        {
            if (!arg.RetryAfter.HasValue)
            {
                return Task.CompletedTask;
            }
            var unixTimeStamp = DateTimeOffset.UtcNow.AddSeconds(arg.RetryAfter.Value + 1).ToUnixTimeSeconds();
            var embed = GeneralHelper.CreateEmbed(
                _parameter.Guild,
                String.Format(GeneralHelper.GetContent("C339", _parameter.Language).Result, unixTimeStamp),
                GeneralHelper.GetCaption("C238", _parameter.Language).Result
                ).Result;

            _parameter.Interaction.RespondAsync(
                embeds: new Embed[] { embed },
                ephemeral: true);

            return Task.CompletedTask;
        }
    }
}
