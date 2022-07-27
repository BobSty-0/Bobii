using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bobii.src.Bobii;
using Bobii.src.Models;
using Discord;

namespace Bobii.src.InteractionModules.ComponentInteractions
{
    public class HelpSelectionMenus : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        [ComponentInteraction("help-selector")]
        public async Task TempChannelHelp(string selected, string[] allChoices)
        {
            var interaction = Context.Interaction;
            var client = Context.Client;
            var parsedUser = (SocketGuildUser)Context.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(parsedUser.Guild.Id).Result;

            await interaction.UpdateAsync(msg => msg.Embeds = new Embed[] {
                            Bobii.Helper.CreateEmbed(interaction, TempChannel.Helper.HelpTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result +
                            TempChannel.Helper.HelpEditTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result, Bobii.Helper.GetCaption("C170", language).Result).Result });
            await Handler.HandlingService.BobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                message: "Temp channel help was chosen", hilfeSection: "Temp Channel");
            await interaction.DeferAsync();
        }
    }
}
