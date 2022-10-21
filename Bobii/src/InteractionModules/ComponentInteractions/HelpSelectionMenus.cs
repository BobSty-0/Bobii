using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Bobii.src.Models;
using Discord;
using Bobii.src.Helper;

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
                            GeneralHelper.CreateEmbed(interaction, TempChannelHelper.HelpTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result +
                            TempChannelHelper.HelpEditTempChannelInfoPart(client.Rest.GetGlobalApplicationCommands().Result, parsedUser.Guild.Id).Result, GeneralHelper.GetCaption("C170", language).Result).Result });
            await Handler.HandlingService.BobiiHelper.WriteToConsol("MessageCom", false, "MessageComponentHandler, Help", new SlashCommandParameter() { Guild = parsedUser.Guild, GuildUser = parsedUser },
                message: "Temp channel help was chosen", hilfeSection: "Temp Channel");
            await interaction.DeferAsync();
        }
    }
}
