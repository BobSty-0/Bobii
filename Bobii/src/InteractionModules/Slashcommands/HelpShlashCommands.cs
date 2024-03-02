using Bobii.src.Bobii;
using Bobii.src.Helper;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.Slashcommands
{
    class HelpShlashCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        [Group("help", "Includes all support commands")]
        public class Help : InteractionModuleBase<ShardedInteractionContext>
        {
            [SlashCommand("commands", "This will show all my commands")]
            public async Task Commands()
            {
                var parameter = Context.ContextToParameter();

                await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                GeneralHelper.GetContent("C015", parameter.Language).Result,
                GeneralHelper.GetCaption("C015", parameter.Language).Result).Result }, components: new ComponentBuilder()
                    .WithSelectMenu(new SelectMenuBuilder()
                        .WithCustomId("help-selector")
                        //Select section here!
                        .WithPlaceholder(GeneralHelper.GetCaption("C016", parameter.Language).Result)
                        .WithOptions(new List<SelectMenuOptionBuilder>
                        {
                new SelectMenuOptionBuilder()
                    //Temporary Voice Channel
                    .WithLabel(GeneralHelper.GetCaption("C017", parameter.Language).Result)
                    .WithValue("temp-channel-help-selectmenuoption")
                    .WithDescription(GeneralHelper.GetContent("C017", parameter.Language).Result),
                new SelectMenuOptionBuilder()
                    //Text Utility
                    .WithLabel(GeneralHelper.GetCaption("C021", parameter.Language).Result)
                    .WithValue("text-utility-help-selectmenuotion")
                    .WithDescription(GeneralHelper.GetContent("C021", parameter.Language).Result),
                new SelectMenuOptionBuilder()
                    //Language
                    .WithLabel(GeneralHelper.GetCaption("C196", parameter.Language).Result)
                    .WithValue("language-help-selectmenuotion")
                    .WithDescription(GeneralHelper.GetContent("C196", parameter.Language).Result),
                        }))
                    .Build());

                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "HelpBobii", parameter, message: "/helpbobii successfully used");
            }

            [SlashCommand("support", "This will give you info on how to reach out to support")]
            public async Task BobiiSupport()
            {
                var parameter = Context.ContextToParameter();

                await parameter.Interaction.RespondAsync("", embeds: new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction, GeneralHelper.HelpSupportPart(parameter.Guild.Id).Result, GeneralHelper.GetCaption("C308", parameter.Language).Result).Result }, components: GeneralHelper.GetSupportButtonComponentBuilder("en", true).Build());
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(BobiiSupport), parameter,
                    message: "/help support successfully used", hilfeSection: "Support");
            }

            [SlashCommand("guides", "This will show all my guides")]
            public async Task BobiiGuides()
            {
                var parameter = Context.ContextToParameter();
                try
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C200", parameter.Language).Result,
                    GeneralHelper.GetCaption("C201", parameter.Language).Result).Result }, components: new ComponentBuilder()
                        .WithSelectMenu(new SelectMenuBuilder()
                            .WithCustomId("guide-selector")
                            .WithPlaceholder(GeneralHelper.GetCaption("C200", parameter.Language).Result)
                            .WithOptions(new List<SelectMenuOptionBuilder>
                            {
                    new SelectMenuOptionBuilder()
                        .WithLabel(GeneralHelper.GetCaption("C199", parameter.Language).Result)
                        .WithValue("how-to-cereate-temp-channel-guide")
                        .WithDescription(GeneralHelper.GetContent("C199", parameter.Language).Result),                            
                   new SelectMenuOptionBuilder()
                        .WithLabel(GeneralHelper.GetCaption("C197", parameter.Language).Result)
                        .WithValue("how-to-text-utility-guide")
                        .WithDescription(GeneralHelper.GetContent("C198", parameter.Language).Result)
                            })).Build());

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
