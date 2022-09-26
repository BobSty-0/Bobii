using Bobii.src.AutocompleteHandler;
using Bobii.src.Bobii;
using Bobii.src.Modals;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.Slashcommands
{
    public class CreateTempChannelSlashCommands : InteractionModuleBase<SocketInteractionContext>
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
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCInfo), parameter, message: "/tcinfo successfully used");
            }

            [SlashCommand("add", "Adds an create-temp-channel")]
            public async Task TCAdd(
                [Summary("createvoicechannel", "Choose the channel which you want to add")][Autocomplete(typeof(TempChannelCreateVoichannelAddHandler))] string createVoiceChannelID,
                [Summary("tempchannelname", "This will be the name of the temp-channel. Note: {username} = Username")] string tempChannelName,
                [Summary("channelsize", "This will be the size of the temp-channel (OPTIONAL)")] int channelSize = 0,
                [Summary("delay", "This will set the delete delay of the temp-channel (OPTIONAL)")] int delay = 0)
            {
                var parameter = Context.ContextToParameter();
                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCAdd)).Result)
                {
                    return;
                }

                if (createVoiceChannelID == Bobii.Helper.GetContent("C095", parameter.Language).Result.ToLower())
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C107", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C107", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCAdd), parameter, message: "Could not find any channels");
                    return;
                }


                if (createVoiceChannelID == "0")
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C107", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C107", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCAdd), parameter, message: "Could not find any channels");
                    return;
                }

                //Checking for valid input and Permission
                if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(TCAdd), true).Result ||
                    Bobii.CheckDatas.CheckIfIDBelongsToVoiceChannel(parameter, createVoiceChannelID, nameof(TCAdd)).Result ||
                    Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDExists(parameter, createVoiceChannelID, nameof(TCAdd)).Result ||
                    Bobii.CheckDatas.CheckNameLength(parameter, createVoiceChannelID, tempChannelName, nameof(TCAdd), 50, true).Result)
                {
                    return;
                }

                try
                {
                    await TempChannel.EntityFramework.CreateTempChannelsHelper.AddCC(parameter.GuildID, tempChannelName, createVoiceChannelID.ToUlong(), channelSize, delay);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C108", parameter.Language).Result, parameter.Guild.GetChannel(createVoiceChannelID.ToUlong()).Name, parameter.GuildUser.Username),
                    Bobii.Helper.GetCaption("C108", parameter.Language).Result).Result });
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCAdd), parameter, createChannelID: createVoiceChannelID.ToUlong(),
                        message: "/tcadd successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C109", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCAdd), parameter, createChannelID: createVoiceChannelID.ToUlong(),
                        message: "Failed to add CreateTempChannel", exceptionMessage: ex.Message);
                    return;
                }
            }

            [Group("update", "Includes all commands to update create temp channels")]
            public class CreateTempChannelUpdate : InteractionModuleBase<SocketInteractionContext>
            {
                [SlashCommand("name", "Updates the temp-channel name of an existing create-temp-channel")]
                public async Task UpdateName(
                    [Summary("createvoicechannel", "Choose the channel which you want to update")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID)
                {
                    var parameter = Context.ContextToParameter();
                    if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(UpdateName)).Result)
                    {
                        return;
                    }

                    if (createVoiceChannelID == Bobii.Helper.GetContent("C096", parameter.Language).Result.ToLower())
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                            Bobii.Helper.GetContent("C110", parameter.Language).Result,
                            Bobii.Helper.GetCaption("C110", parameter.Language).Result).Result },
                            ephemeral: true);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(UpdateName), parameter, message: "Could not find any channels");
                        return;
                    }

                    if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(UpdateName), true).Result ||
                         Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(UpdateName)).Result)
                    {
                        return;
                    }

                    var currentName = CreateTempChannelsHelper.GetCreateTempChannelName(createVoiceChannelID.ToUlong()).Result;

                    var mb = new ModalBuilder()
                        .WithTitle(Bobii.Helper.GetCaption("C173", parameter.Language).Result)
                        .WithCustomId($"createtempchannel_update_name_modal{createVoiceChannelID},{parameter.Language}")
                        .AddTextInput(Helper.GetContent("C170", parameter.Language).Result, "new_name", TextInputStyle.Short, required: true, maxLength: 50, value: currentName);
                    await parameter.Interaction.RespondWithModalAsync(mb.Build());
                }

                [SlashCommand("size", "Updates the temp-channel size of an existing create-temp-channel")]
                public async Task UpdateSize(
                [Summary("createvoicechannel", "Choose the channel which you want to update")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID,
                [Summary("newsize", "Insert the new temp-channel size")] int newSize)
                {
                    var parameter = Context.ContextToParameter();
                    if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(UpdateSize)).Result)
                    {
                        return;
                    }

                    if (createVoiceChannelID == Bobii.Helper.GetContent("C096", parameter.Language).Result.ToLower())
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                            Bobii.Helper.GetContent("C110", parameter.Language).Result,
                            Bobii.Helper.GetCaption("C110", parameter.Language).Result).Result },
                            ephemeral: true);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(UpdateSize), parameter, message: "Could not find any channels");
                        return;
                    }

                    if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(UpdateSize), true).Result ||
                         Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(UpdateSize)).Result)
                    {
                        return;
                    }
                    int? newSizeFinal = null;
                    if (newSize > 99 || newSize < 0)
                    {
                        newSizeFinal = null;
                        await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeTempChannelSize(newSizeFinal, createVoiceChannelID.ToUlong());
                        await parameter.Interaction.RespondAsync(null, new Discord.Embed[] { Bobii.Helper.CreateEmbed(Context.Interaction,
                            string.Format(Bobii.Helper.GetContent("C172", parameter.Language).Result, newSize), Bobii.Helper.GetCaption("C176", parameter.Language).Result).Result});
                    }
                    else
                    {
                        newSizeFinal = newSize;
                        await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeTempChannelSize(newSizeFinal, createVoiceChannelID.ToUlong());
                        await parameter.Interaction.RespondAsync(null, new Discord.Embed[] { Bobii.Helper.CreateEmbed(Context.Interaction,
                            string.Format(Bobii.Helper.GetContent("C173", parameter.Language).Result, newSize), Bobii.Helper.GetCaption("C176", parameter.Language).Result).Result});
                    }
                }

                [SlashCommand("delay", "Updates the temp-channel delay of an existing create-temp-channel")]
                public async Task UpdateDelay(
                [Summary("createvoicechannel", "Choose the channel which you want to update")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID,
                [Summary("newdelay", "Insert the new temp-channel delay time (in seconds)")] int newDelay)
                {
                    var parameter = Context.ContextToParameter();
                    if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(UpdateName)).Result)
                    {
                        return;
                    }

                    if (createVoiceChannelID == Bobii.Helper.GetContent("C096", parameter.Language).Result.ToLower())
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                            Bobii.Helper.GetContent("C110", parameter.Language).Result,
                            Bobii.Helper.GetCaption("C110", parameter.Language).Result).Result },
                            ephemeral: true);
                        await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(UpdateDelay), parameter, message: "Could not find any channels");
                        return;
                    }

                    if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(UpdateDelay), true).Result ||
                         Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(UpdateDelay)).Result)
                    {
                        return;
                    }

                    await CreateTempChannelsHelper.ChangeDelay(newDelay, createVoiceChannelID.ToUlong());
                    await parameter.Interaction.RespondAsync(null, new Discord.Embed[] { Bobii.Helper.CreateEmbed(Context.Interaction,
                            string.Format(Bobii.Helper.GetContent("C174", parameter.Language).Result, newDelay), Bobii.Helper.GetCaption("C177", parameter.Language).Result).Result});
                }
            }

            [SlashCommand("remove", "Removes an create-temp-channel")]
            public async Task TCRemove(
            [Summary("createvoicechannel", "Choose the channel which you want to remove")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID)
            {
                var parameter = Context.ContextToParameter();
                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCRemove)).Result)
                {
                    return;
                }

                if (createVoiceChannelID == Bobii.Helper.GetContent("C096", parameter.Language).Result.ToLower())
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                            Bobii.Helper.GetContent("C110", parameter.Language).Result,
                            Bobii.Helper.GetCaption("C110", parameter.Language).Result).Result },
                        ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCRemove), parameter, message: "Could not find any channels");
                    return;
                }

                if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(TCRemove), true).Result ||
                    Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(TCRemove)).Result)
                {
                    return;
                }

                try
                {
                    await CreateTempChannelsHelper.RemoveCC(parameter.GuildID.ToString(), createVoiceChannelID.ToUlong());
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C116", parameter.Language).Result, parameter.Guild.GetChannel(createVoiceChannelID.ToUlong()).Name, parameter.GuildUser.Username),
                    Bobii.Helper.GetCaption("C116", parameter.Language).Result).Result });
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCRemove), parameter, createChannelID: createVoiceChannelID.ToUlong(),
                        message: "/tcremove successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C117", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(TCRemove), parameter, createChannelID: createVoiceChannelID.ToUlong(),
                        message: "Failed to remove CreateTempChannel", exceptionMessage: ex.Message);
                    return;
                }
            }

            [SlashCommand("createcommandlist", "Creates an embed which shows all the commands to edit temp-channels")]
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
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, false, nameof(TCCreateInfo), parameter, message: "/tccreateinfo successfully used");
            }
        }
    }
}
