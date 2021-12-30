using Bobii.src.Entities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    class SlashCommands
    {
        #region Help
        public static async Task HelpBobii(SlashCommandParameter parameter)
        {
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "I have a lot of commands, so I have divided my commands into sections.\nYou can select the section from which you want to know the commands in the selection-menu below.\nIf you are looking for guides you can use the command: `/bobiiguides`!", "Bobii help:").Result }, components: new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                    .WithCustomId("help-selector")
                    .WithPlaceholder("Select the section here!")
                    .WithOptions(new List<SelectMenuOptionBuilder>
                    {
                new SelectMenuOptionBuilder()
                    .WithLabel("Temporary Voice Channel")
                    .WithValue("temp-channel-help-selectmenuoption")
                    .WithDescription("All my commands to manage temp channels"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Watch 2 Gether (YouTube)")
                    .WithValue("w2g-help-selectmenuoption")
                    .WithDescription("My command to create a watch 2 gether YouTube event"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Filter Word")
                    .WithValue("filter-word-help-selectmenuoption")
                    .WithDescription("All my commands to manage filter words"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Filter Link")
                    .WithValue("filter-link-help-selectmenuotion")
                    .WithDescription("All my commads to manage filter links"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Text Utility")
                    .WithValue("text-utility-help-selectmenuotion")
                    .WithDescription("All my commands to spice up the look of your messages"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Support")
                    .WithValue("support-help-selectmenuotion")
                    .WithDescription("Instruction on my support system"),
                    }))
                .Build());

            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "HelpBobii", parameter, message: "/helpbobii successfully used");
        }
        #endregion

        #region Guide
        public static async Task BobiiGuides(SlashCommandParameter parameter)
        {
            try
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                    "I'm planing on doing more guides in the future but for now there is only a few to select in the select-menu below.\nYou can select the guide you wish to follow in the selection-menu.\nIf you are looking for commands, you can use the command: `/helpbobii`!", "Bobii guides:").Result }, components: new ComponentBuilder()
                    .WithSelectMenu(new SelectMenuBuilder()
                        .WithCustomId("guide-selector")
                        .WithPlaceholder("Select the guide here!")
                        .WithOptions(new List<SelectMenuOptionBuilder>
                        {
                    new SelectMenuOptionBuilder()
                        .WithLabel("Temp channel guide")
                        .WithValue("how-to-cereate-temp-channel-guide")
                        .WithDescription("Guide for all commands to manage create-temp-channel"),
                    new SelectMenuOptionBuilder()
                        .WithLabel("Add a link to the whitelist")
                        .WithValue("how-to-add-filter-link-guide")
                        .WithDescription("Guide for /flladd")
                        })
                        ).Build());

                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "BobiiGuides", parameter, message: "/bobiiguides successfully used");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Guild Utility
        public static async Task Refresh(SlashCommandParameter parameter)
        {
            try
            {
                if (CheckDatas.CheckIfItsBobSty(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand.Data, "Refresh", false).Result)
                {
                    return;
                }

                await Bobii.Helper.RefreshBobiiStats();
                await src.Handler.HandlingService.ResetCache();
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "The stats channels should be refreshed", "Successfully refreshed!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "Refresh", parameter, message: "/refresh successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "Refresh", parameter, message: "/refresh could not be used", exceptionMessage: ex.Message);
            }
        }

        public static async Task ServerCount(SlashCommandParameter parameter)
        {
            try
            {
                if (CheckDatas.CheckIfItsBobSty(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand.Data, "Refresh", false).Result)
                {
                    return;
                }
                await parameter.Interaction.RespondAsync(null, new Embed[]
                { Bobii.Helper.CreateEmbed(parameter.Interaction, Bobii.Helper.CreateServerCount(parameter.Client).Result, "Here is a list of all the servers I'm in!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "ServerCount", parameter, message: "/servercount successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "ServerCount", parameter, message: "/servercount could not be used", exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
