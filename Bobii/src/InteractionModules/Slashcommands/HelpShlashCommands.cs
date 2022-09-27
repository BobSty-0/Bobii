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
            [SlashCommand("commands", "This will show all my commands")]
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
                    //Text Utility
                    .WithLabel(Helper.GetCaption("C021", parameter.Language).Result)
                    .WithValue("text-utility-help-selectmenuotion")
                    .WithDescription(Helper.GetContent("C021", parameter.Language).Result),
                        }))
                    .Build());

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "HelpBobii", parameter, message: "/helpbobii successfully used");
            }

            [SlashCommand("support", "This will give you info on how to reach out to support")]
            public async Task BobiiSupport()
            {
                var parameter = Context.ContextToParameter();

                await parameter.Interaction.RespondAsync("", new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, Bobii.Helper.HelpSupportPart(parameter.Guild.Id).Result, "Support:").Result });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(BobiiSupport), parameter,
                    message: "/help support successfully used", hilfeSection: "Support");
            }

            [SlashCommand("guides", "This will show all my guides")]
            public async Task BobiiGuides()
            {
                var parameter = Context.ContextToParameter();
                try
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    "I'm planing on doing more guides in the future but for now there is only one to select in the select-menu below.\nYou can select the guide you wish to follow in the selection-menu.\nIf you are looking for commands, you can use the command: `/help commands`!", "Bobii guides:").Result }, components: new ComponentBuilder()
                        .WithSelectMenu(new SelectMenuBuilder()
                            .WithCustomId("guide-selector")
                            .WithPlaceholder("Select the guide here!")
                            .WithOptions(new List<SelectMenuOptionBuilder>
                            {
                    new SelectMenuOptionBuilder()
                        .WithLabel("Temp channel guide")
                        .WithValue("how-to-cereate-temp-channel-guide")
                        .WithDescription("Guide for all commands to manage create-temp-channel")
                            })
                            ).Build());

                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "BobiiGuides", parameter, message: "/bobiiguides successfully used");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
