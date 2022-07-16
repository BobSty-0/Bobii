using Bobii.src.Bobii;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.Slashcommands
{
    class HelpShlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("help", "Includes all support commands")]
        public class Help : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("commands", "Returns a list of all commands from Bobii ")]
            public async Task Commands()
            {
                var parameter = Context.ContextToParameter();

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                Helper.GetContent("C015", parameter.Language).Result,
                Helper.GetCaption("C015", parameter.Language).Result).Result }, components: new ComponentBuilder()
                    .WithSelectMenu(new SelectMenuBuilder()
                        .WithCustomId("help-selector")
                        //Select section here!
                        .WithPlaceholder(Helper.GetCaption("C016", parameter.Language).Result)
                        .WithOptions(new List<SelectMenuOptionBuilder>
                        {
                new SelectMenuOptionBuilder()
                    //Temporary Voice Channel
                    .WithLabel(Helper.GetCaption("C017", parameter.Language).Result)
                    .WithValue("temp-channel-help-selectmenuoption")
                    .WithDescription(Helper.GetContent("C017", parameter.Language).Result),
                new SelectMenuOptionBuilder()
                    //Link Filter
                    .WithLabel(Helper.GetCaption("C020", parameter.Language).Result)
                    .WithValue("filter-link-help-selectmenuotion")
                    .WithDescription(Helper.GetContent("C020", parameter.Language).Result),
                new SelectMenuOptionBuilder()
                    //Text Utility
                    .WithLabel(Helper.GetCaption("C021", parameter.Language).Result)
                    .WithValue("text-utility-help-selectmenuotion")
                    .WithDescription(Helper.GetContent("C021", parameter.Language).Result),
                        }))
                    .Build());

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "HelpBobii", parameter, message: "/helpbobii successfully used");
            }
        }
    }
}
