using Bobii.src.Models;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    class SlashCommands
    {
        #region Test
        public static async Task Test(SlashCommandParameter parameter)
        {
            var components = new List<IMessageComponent>();

            var dropDownMenu = new ComponentBuilder()
                    .WithSelectMenu(new SelectMenuBuilder()
                        .WithCustomId("hello")
                        .WithPlaceholder("Select voice channel here")
                        .WithOptions(GetVoiceChannels(parameter).Result)
                        ).Build();

            components.Add((IMessageComponent)dropDownMenu);

            var mb = new ModalBuilder()
                .WithTitle("Add a create-temp-channel")
                .WithCustomId("tcadd_modal")
                .AddComponents(components: components, 0)
                .AddTextInput("What??", "food_name", placeholder: "Pizza")
                .AddTextInput("Why??", "food_reason", TextInputStyle.Paragraph,
                    "Kus it's so tasty");

            await parameter.Interaction.RespondWithModalAsync(mb.Build());

        }

        public static async Task<List<SelectMenuOptionBuilder>> GetVoiceChannels(SlashCommandParameter parameter)
        {
            var selectMenuOptions = new List<SelectMenuOptionBuilder>();
            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(parameter.Guild);
            foreach (var channel in parameter.Guild.VoiceChannels)
            {


                var createTempChannel = createTempChannels.Result.Where(ch => ch.createchannelid == channel.Id).FirstOrDefault();
                if (createTempChannel != null)
                {
                    continue;
                }
                var selectMenuOption = new SelectMenuOptionBuilder()
                    .WithLabel($"{channel.Name} - ID: {channel.Id}")
                    .WithValue($"{channel.Id}")
                    .WithDescription("test");
                selectMenuOptions.Add(selectMenuOption);
            }
            return selectMenuOptions;
        }
        #endregion

        #region Help
        public static async Task BobiiHelp(SlashCommandParameter parameter)
        {
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

            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "HelpBobii", parameter, message: "/helpbobii successfully used");
        }
        #endregion

        #region Guide
        public static async Task BobiiSupport(SlashCommandParameter parameter)
        {
            await parameter.Interaction.RespondAsync("", new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, Bobii.Helper.HelpSupportPart(parameter.Guild.Id).Result, "Support:").Result });
            await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(BobiiSupport), parameter,
                message: "/help support successfully used", hilfeSection: "Support");
        }

        public static async Task BobiiGuides(SlashCommandParameter parameter)
        {
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

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "BobiiGuides", parameter, message: "/bobiiguides successfully used");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Guild Utility
        public static async Task Backup(SlashCommandParameter parameter)
        {
            try
            {
                if (CheckDatas.CheckIfItsBobSty(parameter, nameof(Backup), false).Result)
                {
                    return;
                }

                await Helper.DoUpdate(parameter, Enums.DatabaseConnectionString.ConnectionString);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        "Prachcode einbauen, hat geklappt",
                        "Sprachcode einbauen" ).Result});

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(Backup), parameter, message: "Successfully used /backup");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(Backup), parameter, message: "/backup could not be used", exceptionMessage: ex.Message);
            }
        }

        public static async Task Refresh(SlashCommandParameter parameter)
        {
            try
            {
                if (CheckDatas.CheckIfItsBobSty(parameter, "Refresh", false).Result)
                {
                    return;
                }

                await Bobii.Helper.RefreshBobiiStats();
                await src.Handler.HandlingService.ResetCache();
                _ = Task.Run(async () => TempChannel.Helper.CheckForTempChannelCorps(parameter));

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Helper.GetContent("C013", parameter.Language).Result,
                        Helper.GetCaption("C013", parameter.Language).Result).Result });

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "Refresh", parameter, message: "Successfully used /refresh");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "Refresh", parameter, message: "/refresh could not be used", exceptionMessage: ex.Message);
            }
        }

        public static async Task ServerCount(SlashCommandParameter parameter)
        {
            try
            {
                if (CheckDatas.CheckIfItsBobSty(parameter, "Refresh", false).Result)
                {
                    return;
                }
                var path = $"Servercount_{DateTime.Now}.md";
                path = path.Replace(' ', '_');
                path = path.Replace(':', '.');
                path = path.Replace('/', '.');

                using (FileStream fs = File.Create(path))
                {
                    path = fs.Name;
                }

                using (StringReader reader = new StringReader(Bobii.Helper.CreateServerCount(parameter.Client).Result))
                {
                    using (var tw = new StreamWriter(path, true))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            tw.WriteLine(line);
                        }
                    }
                }

                await parameter.Interaction.RespondAsync("Here is the server list:");
                await parameter.Interaction.Channel.SendFileAsync(path);

                File.Delete(path);

                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, "ServerCount", parameter, message: "/servercount successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, "ServerCount", parameter, message: "/servercount could not be used", exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
