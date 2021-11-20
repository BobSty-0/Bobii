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
            await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "I have a lot of commands, so I have divided my commands into sections.\nYou can select the section from which you want to know the commands in the selection-menu below.\nIf you are looking for guides you can use the command: `/bobiiguides`!", "Bobii help:").Result }, component: new ComponentBuilder()
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
                    .WithLabel("Filter Word")
                    .WithValue("filter-word-help-selectmenuoption")
                    .WithDescription("All my commands to manage filter words"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Filter Link")
                    .WithValue("filter-link-help-selectmenuotion")
                    .WithDescription("All my commads to manage filter links"),
                new SelectMenuOptionBuilder()
                    .WithLabel("Support")
                    .WithValue("support-help-selectmenuotion")
                    .WithDescription("Instruction on my support system")
                    }))
                .Build());
            await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: Help | Guild: {parameter.GuildID} | /helpbobii successfully used");
        }
        #endregion

        #region Guide
        public static async Task BobiiGuides(SlashCommandParameter parameter)
        {
            try
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "I'm planing on doing more guides in the future but for now there is only a few to select in the select-menu below.\nYou can select the guid you wish to follow in the selection-menu.\nIf you are looking for commands, you can use the command: `/helpbobii`!", "Bobii guides:").Result }, component: new ComponentBuilder()
                    .WithSelectMenu(new SelectMenuBuilder()
                        .WithCustomId("guide-selector")
                        .WithPlaceholder("Select the guide here!")
                        .WithOptions(new List<SelectMenuOptionBuilder>
                        {
                    new SelectMenuOptionBuilder()
                        .WithLabel("Add create-temp-channel")
                        .WithValue("how-to-cereate-temp-channel-guide")
                        .WithDescription("Guide for /tcadd"),
                    new SelectMenuOptionBuilder()
                        .WithLabel("Add a link to the whitelist")
                        .WithValue("how-to-add-filter-link-guide")
                        .WithDescription("Guide for /flladd") 
                        })                        
                        ).Build());
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: Guides | Guild {parameter.GuildID} | /bobiiguides successfully used");
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
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "The stats channels should be refreshed", "Successfully refreshed!").Result });
            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error | /refresh could not be used | {ex.Message}");
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
            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error | /refresh could not be used | {ex.Message}");
            }
        }
        #endregion
    }
}
