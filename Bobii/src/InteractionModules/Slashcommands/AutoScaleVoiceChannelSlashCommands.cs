using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;
using Bobii.src.Bobii;
using Bobii.src.Helper;
using Bobii.src.AutocompleteHandler;
using Bobii.src.TempChannel.EntityFramework;
using Bobii.src.Handler;
using System.Linq;
using System.Collections.Generic;

namespace src.InteractionModules.Slashcommands
{
    public class AutoScaleVoiceChannelCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("autoscale", "Includes all commands to edit auto scaling voice channels ")]
        public class CreateTempChannel : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("setup", "(BETA) Sets an auto scaling category with voice channels up")]
            public async Task AutoScalingVoicesSetup()
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.AutoScalingVoiceSetup(parameter);
            }
        }
    }
}
