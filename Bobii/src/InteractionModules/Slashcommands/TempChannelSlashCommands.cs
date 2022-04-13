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
    public class TempChannelSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("createtempchannel", "Includes all commands to edit create temp channels")]
        public class CreateTempChannel : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("info", "Returns a list of all the create-temp-channels of this Guild")]
            public async Task TCInfo()
            {
                var parameter = Context.ContextToParameter();
                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCInfo)).Result)
                {
                    return;
                }
                await parameter.Interaction.RespondAsync("", new Embed[] { TempChannel.Helper.CreateVoiceChatInfoEmbed(parameter) });
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCInfo), parameter, message: "/tcinfo successfully used");
            }

            [SlashCommand("creatcommandlist", "Creates an embed which shows all the commands to edit temp-channels")]
            public async Task TCCreateInfo()
            {
                var parameter = Context.ContextToParameter();
                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCCreateInfo)).Result)
                {
                    return;
                }
                await parameter.Interaction.Channel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(parameter.Interaction,
                    TempChannel.Helper.HelpEditTempChannelInfoPart(parameter.Client.Rest.GetGlobalApplicationCommands().Result, parameter.GuildID, true).Result,
                    Bobii.Helper.GetContent("C106", parameter.Language).Result).Result);

                await parameter.Interaction.DeferAsync();
                await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
                await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCCreateInfo), parameter, message: "/tccreateinfo successfully used");
            }

            [SlashCommand("add", "Adds an create-temp-channel")]
            public async Task TCAdd(
                [Summary("createvoicechannel", "Choose the channel which you want to add")][Autocomplete()] string createVoiceChannel,
                [Summary("tempchannelname", "This will be the name of the temp-channel. Note: {username} = Username")] string tempChannelName,
                [Summary("channelsize", "This will be the size of the temp-channel (OPTIONAL)")] int channelSize = 0,
                [Summary("textchannel", "Bobii will create an additional temp-text-channel if activated (OPTIONAL)")] bool textChannel = false,
                [Summary("delay", "This will set the delete delay of the temp-channel (OPTIONAL)")] int delay = 0)
            {
                var parameter = Context.ContextToParameter();
                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCAdd)).Result)
                {
                    return;
                }

                var nameAndID = createVoiceChannel.Split(" ");


                if (createVoiceChannel == Bobii.Helper.GetContent("C095", parameter.Language).Result.ToLower())
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C107", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C107", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCAdd), parameter, message: "Could not find any channels");
                    return;
                }

                var createChannelID = nameAndID[nameAndID.Count() - 1];

                //Checking for valid input and Permission
                if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, nameof(TCAdd), true).Result ||
                    Bobii.CheckDatas.CheckIfIDBelongsToVoiceChannel(parameter, createChannelID, nameof(TCAdd)).Result ||
                    Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDExists(parameter, createChannelID, nameof(TCAdd)).Result ||
                    Bobii.CheckDatas.CheckNameLength(parameter, createChannelID, tempChannelName, nameof(TCAdd), 50, true).Result)
                {
                    return;
                }

                try
                {
                    await TempChannel.EntityFramework.CreateTempChannelsHelper.AddCC(parameter.GuildID, tempChannelName, ulong.Parse(createChannelID), channelSize, textChannel, delay);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C108", parameter.Language).Result, parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name, parameter.GuildUser.Username),
                    Bobii.Helper.GetCaption("C108", parameter.Language).Result).Result });
                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCAdd), parameter, createChannelID: ulong.Parse(createChannelID),
                        message: "/tcadd successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C109", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCAdd), parameter, createChannelID: ulong.Parse(createChannelID),
                        message: "Failed to add CreateTempChannel", exceptionMessage: ex.Message);
                    return;
                }
            }

            [Group("update", "Includes all commands to update create temp channels")]
            public class CreateTempChannelUpdate : InteractionModuleBase<SocketInteractionContext>
            {
                [SlashCommand("name", "Updates the temp-channel name of an existing create-temp-channel")]
                public async Task UpdateName(
                    [Summary("createvoicechannel_name", "Choose the channel which you want to update")][Autocomplete()] string createVoiceChannel)
                {
                    var parameter = Context.ContextToParameter();
                    var nameAndID = createVoiceChannel.Split(" ");
                    var createChannelID = nameAndID[nameAndID.Count() - 1];

                    if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(UpdateName)).Result ||
                        Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, nameof(UpdateName), true).Result ||
                         Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createChannelID, nameof(UpdateName)).Result)
                    {
                        return;
                    }

                    var mb = new ModalBuilder()
                        .WithTitle("Change create temp channel name!")
                        .WithCustomId("tcupdate_name_modal")
                        .AddTextInput("New name:", "content", TextInputStyle.Short, required: true, maxLength: 25);

                    await parameter.Interaction.RespondWithModalAsync(mb.Build());
                }
            }

            //[SlashCommand("update", "Updates the temp-channel name or/and size or/and textchannel of an existing create-temp-channel")]
            //public async Task TCUpdate()
            //{
            //    var parameter = Context.ContextToParameter();
            //    if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCUpdate)).Result)
            //    {
            //        return;
            //    }

            //    bool textChannelNewb = false;
            //    var sb = new StringBuilder();
            //    var createVoiceChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "createvoicechannel").Result.String;
            //    var newTempChannelName = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newtempchannelname").Result.String;
            //    var newChannelSize = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newtempchannelsize").Result.Integer;
            //    var newTextChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "textchannel").Result.String;
            //    var delay = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "delay").Result.Integer;

            //    int? finalDelay = null;
            //    if (delay != 0)
            //    {
            //        finalDelay = delay;
            //    }

            //    var nameAndID = createVoiceChannel.Split(" ");
            //    var createChannelID = nameAndID[nameAndID.Count() - 1];

            //    if (createVoiceChannel == Bobii.Helper.GetContent("C096", parameter.Language).Result.ToLower())
            //    {
            //        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
            //        Bobii.Helper.GetContent("C110", parameter.Language).Result,
            //        Bobii.Helper.GetCaption("C110", parameter.Language).Result).Result },
            //            ephemeral: true);
            //        await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCUpdate), parameter, message: "Could not find any channels");
            //        return;
            //    }

            //    // Only the create-temp-channel was choosen
            //    if (parameter.SlashCommandData.Options.Count() == 1)
            //    {
            //        return;
            //    }

            //    if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, nameof(TCUpdate), true).Result ||
            //         Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createChannelID, nameof(TCUpdate)).Result)
            //    {
            //        return;
            //    }

            //    try
            //    {
            //        if (newTempChannelName != null)
            //        {
            //            if (Bobii.CheckDatas.CheckNameLength(parameter, createChannelID, newTempChannelName, nameof(TCUpdate), 50, true).Result)
            //            {
            //                return;
            //            }

            //            await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeTempChannelName(newTempChannelName, ulong.Parse(createChannelID));
            //            sb.AppendLine();
            //            sb.AppendLine(string.Format(Bobii.Helper.GetContent("C111", parameter.Language).Result, newTempChannelName));
            //        }

            //        if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "newtempchannelsize").FirstOrDefault() != null)
            //        {
            //            await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeTempChannelSize(newChannelSize, ulong.Parse(createChannelID));
            //            sb.AppendLine();
            //            sb.AppendLine(string.Format(Bobii.Helper.GetContent("C112", parameter.Language).Result, newChannelSize));
            //        }

            //        if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "textchannel").FirstOrDefault() != null)
            //        {
            //            if (newTextChannel == "on")
            //            {
            //                textChannelNewb = true;
            //            }
            //            await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeTextChannel(textChannelNewb, ulong.Parse(createChannelID));
            //            sb.AppendLine();
            //            sb.AppendLine(string.Format(Bobii.Helper.GetContent("C113", parameter.Language).Result, newTextChannel));
            //        }

            //        if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "delay").FirstOrDefault() != null)
            //        {
            //            await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeDelay(finalDelay, ulong.Parse(createChannelID));
            //            sb.AppendLine();
            //            sb.AppendLine($"Delay successfully changed to {delay} minutes.");
            //        }

            //        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
            //        sb.ToString(),
            //        Bobii.Helper.GetCaption("C111", parameter.Language).Result).Result });
            //        await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCUpdate), parameter, createChannelID: ulong.Parse(createChannelID),
            //            message: "/tcupdate successfully used");
            //    }
            //    catch (Exception ex)
            //    {
            //        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
            //        Bobii.Helper.GetContent("C114", parameter.Language).Result,
            //        Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
            //        await Handler.HandlingService._bobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCUpdate), parameter, createChannelID: ulong.Parse(createChannelID),
            //            message: "Failed to update create-temp-channel", exceptionMessage: ex.Message);
            //        return;
            //    }

            //    await Task.CompletedTask;
            //}
        }

    }
}
