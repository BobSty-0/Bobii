using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;
using Bobii.src.Bobii;
using Bobii.src.Helper;
using Bobii.src.AutocompleteHandler;
using Bobii.src.TempChannel.EntityFramework;
using Bobii.src.Handler;
using System.Linq;
using System.Collections.Generic;

namespace src.InteractionModules.Slashcommands
{
    public class CreateTempChannelSlashCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        [Group("creator", "Includes all commands to edit creator channels")]
        public class CreateTempChannel : InteractionModuleBase<ShardedInteractionContext>
        {
            [SlashCommand("info", "Returns detailed information about a existing creator channels")]
            public async Task TCInfo()
            {
                var parameter = Context.ContextToParameter();
                if (CheckDatas.CheckUserPermission(parameter, nameof(TCInfo)).Result)
                {
                    return;
                }

                var createTempChannels = CreateTempChannelsHelper.GetCreateTempChannelList().Result.Where(c => c.guildid == parameter.GuildID).ToList();
                if (createTempChannels.Count() == 0)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C100", parameter.Language).Result,
                    GeneralHelper.GetCaption("C238", parameter.Language).Result).Result }, ephemeral: true);
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TCInfo), parameter, message: "/tcinfo keine create-temp-channels");
                }
                else
                {
                    var list = new List<SelectMenuOptionBuilder>();
                    foreach(var channel in createTempChannels)
                    {
                        var createChannel = (IVoiceChannel)parameter.Client.GetChannel(channel.createchannelid);
                        if (createChannel == null)
                        {
                            continue;
                        }

                        list.Add(
                            new SelectMenuOptionBuilder()
                                .WithValue(channel.createchannelid.ToString())
                                .WithLabel(createChannel.Name));
                    }

                    if (list.Count() == 0)
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C100", parameter.Language).Result,
                            GeneralHelper.GetCaption("C238", parameter.Language).Result).Result }, ephemeral: true);
                        await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TCInfo), parameter, message: "/tcinfo keine create-temp-channels");
                        return;
                    }

                    var menuBuilder = new SelectMenuBuilder()
                        .WithPlaceholder(GeneralHelper.GetContent("C247", parameter.Language).Result)
                        .WithMinValues(1)
                        .WithMaxValues(1)
                        .WithCustomId("create-temp-channel-info")
                        .WithType(ComponentType.SelectMenu)
                        .WithOptions(list);

                    await parameter.Interaction.RespondAsync("", components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build());
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TCInfo), parameter, message: "/tcinfo menu created");
                }
            }

            [SlashCommand("setup", "Sets an create-temp-channel up")]
            public async Task TCSetup() 
            {
                var parameter = Context.ContextToParameter();
                await TempChannelHelper.TempChannelSetup(parameter);
            }

            [SlashCommand("add", "Adds an creator channel")]
            public async Task TCAdd(
                [Summary("createvoicechannel", "Choose the channel which you want to add")][Autocomplete(typeof(TempChannelCreateVoichannelAddHandler))] string createVoiceChannelID,
                [Summary("tempchannelname", "This will be the name of the temp channel. Note: {username} = Username")] string tempChannelName,
                [Summary("channelsize", "This will be the size of the temp-channel (OPTIONAL)")] int channelSize = 0,
                [Summary("delay", "This will set the delete delay of the temp-channel (OPTIONAL)")] int delay = 0,
                [Summary("autodeletemessages", "This sets the time after which the messages in the temp channel chat are deleted (OPTIONAL)")] int autodelete = 0)
            {
                var parameter = Context.ContextToParameter();
                if (CheckDatas.CheckUserPermission(parameter, nameof(TCAdd)).Result)
                {
                    return;
                }

                if (createVoiceChannelID == GeneralHelper.GetContent("C095", parameter.Language).Result.ToLower())
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C107", parameter.Language).Result,
                    GeneralHelper.GetCaption("C107", parameter.Language).Result).Result }, ephemeral: true);
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TCAdd), parameter, message: "Could not find any channels");
                    return;
                }


                if (createVoiceChannelID == "0")
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C107", parameter.Language).Result,
                    GeneralHelper.GetCaption("C107", parameter.Language).Result).Result }, ephemeral: true);
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TCAdd), parameter, message: "Could not find any channels");
                    return;
                }

                //Checking for valid input and Permission
                if (CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(TCAdd), true).Result ||
                    CheckDatas.CheckIfIDBelongsToVoiceChannel(parameter, createVoiceChannelID, nameof(TCAdd)).Result ||
                    CheckDatas.CheckIfCreateTempChannelWithGivenIDExists(parameter, createVoiceChannelID, nameof(TCAdd)).Result ||
                    CheckDatas.CheckNameLength(parameter, createVoiceChannelID, tempChannelName, nameof(TCAdd), 50, true).Result ||
                    CheckDatas.CheckAutodelteSize(parameter, autodelete.ToString(), nameof(TCAdd)).Result ||
                    CheckDatas.CheckDelaySize(parameter, delay, nameof(TCAdd)).Result)
                {
                    return;
                }

                try
                {
                    await CreateTempChannelsHelper.AddCC(parameter.GuildID, tempChannelName, createVoiceChannelID.ToUlong(), channelSize, delay, autodelete);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C108", parameter.Language).Result, parameter.Guild.GetChannel(createVoiceChannelID.ToUlong()).Name, parameter.GuildUser.Username),
                    GeneralHelper.GetCaption("C108", parameter.Language).Result).Result });
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TCAdd), parameter, createChannelID: createVoiceChannelID.ToUlong(),
                        message: "/tcadd successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C109", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TCAdd), parameter, createChannelID: createVoiceChannelID.ToUlong(),
                        message: "Failed to add CreateTempChannel", exceptionMessage: ex.Message);
                    return;
                }
            }

            [Group("update", "Includes all commands to update create temp channels")]
            public class CreateTempChannelUpdate : InteractionModuleBase<ShardedInteractionContext>
            {
                [SlashCommand("name", "Updates the temp channel name of an existing creator channel")]
                public async Task UpdateName(
                    [Summary("createvoicechannel", "Choose the channel which you want to update")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID)
                {
                    var parameter = Context.ContextToParameter();
                    if (CheckDatas.CheckUserPermission(parameter, nameof(UpdateName)).Result)
                    {
                        return;
                    }

                    if (createVoiceChannelID == GeneralHelper.GetContent("C096", parameter.Language).Result.ToLower())
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C110", parameter.Language).Result,
                            GeneralHelper.GetCaption("C110", parameter.Language).Result).Result },
                            ephemeral: true);
                        await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(UpdateName), parameter, message: "Could not find any channels");
                        return;
                    }

                    if (CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(UpdateName), true).Result ||
                         CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(UpdateName)).Result)
                    {
                        return;
                    }

                    var currentName = CreateTempChannelsHelper.GetCreateTempChannelName(createVoiceChannelID.ToUlong()).Result;

                    var mb = new ModalBuilder()
                        .WithTitle(GeneralHelper.GetCaption("C173", parameter.Language).Result)
                        .WithCustomId($"createtempchannel_update_name_modal{createVoiceChannelID},{parameter.Language}")
                        .AddTextInput(GeneralHelper.GetContent("C170", parameter.Language).Result, "new_name", TextInputStyle.Short, required: true, maxLength: 50, value: currentName);
                    await parameter.Interaction.RespondWithModalAsync(mb.Build());
                }

                [SlashCommand("size", "Updates the temp channel size of an existing creator channel")]
                public async Task UpdateSize(
                [Summary("createvoicechannel", "Choose the channel which you want to update")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID,
                [Summary("newsize", "Insert the new temp-channel size")] int newSize)
                {
                    var parameter = Context.ContextToParameter();
                    if (CheckDatas.CheckUserPermission(parameter, nameof(UpdateSize)).Result)
                    {
                        return;
                    }

                    if (createVoiceChannelID == GeneralHelper.GetContent("C096", parameter.Language).Result.ToLower())
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C110", parameter.Language).Result,
                            GeneralHelper.GetCaption("C110", parameter.Language).Result).Result },
                            ephemeral: true);
                        await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(UpdateSize), parameter, message: "Could not find any channels");
                        return;
                    }

                    if (CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(UpdateSize), true).Result ||
                         CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(UpdateSize)).Result)
                    {
                        return;
                    }
                    int? newSizeFinal = null;
                    if (newSize > 99 || newSize < 0)
                    {
                        newSizeFinal = null;
                        await CreateTempChannelsHelper.ChangeTempChannelSize(newSizeFinal, createVoiceChannelID.ToUlong());
                        await parameter.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                            string.Format(GeneralHelper.GetContent("C172", parameter.Language).Result, newSize), GeneralHelper.GetCaption("C176", parameter.Language).Result).Result});
                    }
                    else
                    {
                        newSizeFinal = newSize;
                        await CreateTempChannelsHelper.ChangeTempChannelSize(newSizeFinal, createVoiceChannelID.ToUlong());
                        await parameter.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                            string.Format(GeneralHelper.GetContent("C173", parameter.Language).Result, newSize), GeneralHelper.GetCaption("C176", parameter.Language).Result).Result});
                    }
                }

                [SlashCommand("delay", "Updates the temp channel delay of an existing creator channel")]
                public async Task UpdateDelay(
                [Summary("createvoicechannel", "Choose the channel which you want to update")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID,
                [Summary("newtime", "Insert the new temp-channel delay time (in minutes)")] int newDelay)
                {
                    var parameter = Context.ContextToParameter();
                    if (CheckDatas.CheckUserPermission(parameter, nameof(UpdateName)).Result)
                    {
                        return;
                    }

                    if (createVoiceChannelID == GeneralHelper.GetContent("C096", parameter.Language).Result.ToLower())
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C110", parameter.Language).Result,
                            GeneralHelper.GetCaption("C110", parameter.Language).Result).Result },
                            ephemeral: true);
                        await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(UpdateDelay), parameter, message: "Could not find any channels");
                        return;
                    }

                    if (CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(UpdateDelay), true).Result ||
                         CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(UpdateDelay)).Result ||
                         CheckDatas.CheckDelaySize(parameter, newDelay, nameof(UpdateAutodelete)).Result)
                    {
                        return;
                    }

                    await CreateTempChannelsHelper.ChangeDelay(newDelay, createVoiceChannelID.ToUlong());
                    await parameter.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                            string.Format(GeneralHelper.GetContent("C174", parameter.Language).Result, newDelay), GeneralHelper.GetCaption("C177", parameter.Language).Result).Result});
                }

                [SlashCommand("autodeletemessages", "Updates the time after which the messages in the temp channel chat are automatically deleted")]
                public async Task UpdateAutodelete(
                    [Summary("createvoicechannel", "Choose the channel which you want to update")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID,
                    [Summary("newtime", "Insert the new time after which messages should be deleted (in minutes)")] int newautodelete)
                {
                    var parameter = Context.ContextToParameter();
                    if (CheckDatas.CheckUserPermission(parameter, nameof(UpdateAutodelete)).Result)
                    {
                        return;
                    }

                    if (createVoiceChannelID == GeneralHelper.GetContent("C096", parameter.Language).Result.ToLower())
                    {
                        await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C110", parameter.Language).Result,
                            GeneralHelper.GetCaption("C110", parameter.Language).Result).Result },
                            ephemeral: true);
                        await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(UpdateAutodelete), parameter, message: "Could not find any channels");
                        return;
                    }

                    if (CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(UpdateAutodelete), true).Result ||
                         CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(UpdateAutodelete)).Result ||
                         CheckDatas.CheckAutodelteSize(parameter, newautodelete.ToString(), nameof(UpdateAutodelete)).Result)
                    {
                        return;
                    }

                    await CreateTempChannelsHelper.ChangeAutodelete(newautodelete, createVoiceChannelID.ToUlong());
                    await parameter.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                            string.Format(GeneralHelper.GetContent("C174", parameter.Language).Result, newautodelete), GeneralHelper.GetCaption("C236", parameter.Language).Result).Result});
                }
            }

            [SlashCommand("remove", "Removes an create-temp-channel")]
            public async Task TCRemove(
            [Summary("createvoicechannel", "Choose the channel which you want to remove")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID)
            {
                var parameter = Context.ContextToParameter();
                if (CheckDatas.CheckUserPermission(parameter, nameof(TCRemove)).Result)
                {
                    return;
                }

                if (createVoiceChannelID == GeneralHelper.GetContent("C096", parameter.Language).Result.ToLower())
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C110", parameter.Language).Result,
                            GeneralHelper.GetCaption("C110", parameter.Language).Result).Result },
                        ephemeral: true);
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TCRemove), parameter, message: "Could not find any channels");
                    return;
                }

                if (CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(TCRemove), true).Result ||
                    CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(TCRemove)).Result)
                {
                    return;
                }

                try
                {
                    await CreateTempChannelsHelper.RemoveCC(parameter.GuildID.ToString(), createVoiceChannelID.ToUlong());
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    string.Format(GeneralHelper.GetContent("C116", parameter.Language).Result, parameter.Guild.GetChannel(createVoiceChannelID.ToUlong()).Name, parameter.GuildUser.Username),
                    GeneralHelper.GetCaption("C116", parameter.Language).Result).Result });
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TCRemove), parameter, createChannelID: createVoiceChannelID.ToUlong(),
                        message: "/tcremove successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C117", parameter.Language).Result,
                    GeneralHelper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TCRemove), parameter, createChannelID: createVoiceChannelID.ToUlong(),
                        message: "Failed to remove CreateTempChannel", exceptionMessage: ex.Message);
                    return;
                }
            }

            [SlashCommand("createcommandlist", "Creates an embed which shows all the commands to edit temp-channels")]
            public async Task TCCreateInfo(
                [Summary("createvoicechannel", "Choose the channel which you want to create the commands list for")][Autocomplete(typeof(TempChannelCreateVoichannelUpdateHandler))] string createVoiceChannelID)
            {
                var parameter = Context.ContextToParameter();
                if (CheckDatas.CheckUserPermission(parameter, nameof(TCCreateInfo)).Result)
                {
                    return;
                }

                if (createVoiceChannelID == GeneralHelper.GetContent("C096", parameter.Language).Result.ToLower())
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                            GeneralHelper.GetContent("C110", parameter.Language).Result,
                            GeneralHelper.GetCaption("C110", parameter.Language).Result).Result },
                        ephemeral: true);
                    await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, true, nameof(TCCreateInfo), parameter, message: "Could not find any channels");
                    return;
                }

                if (CheckDatas.CheckDiscordChannelIDFormat(parameter, createVoiceChannelID, nameof(TCCreateInfo), true).Result ||
                     CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter, createVoiceChannelID, nameof(TCCreateInfo)).Result)
                {
                    return;
                }

                await parameter.Interaction.Channel.SendMessageAsync(embed: GeneralHelper.CreateEmbed(parameter.Interaction,
                    TempChannelHelper.HelpEditTempChannelInfoPart(parameter.Client.Rest.GetGlobalApplicationCommands().Result, parameter.GuildID, true, createVoiceChannelID).Result,
                    GeneralHelper.GetContent("C106", parameter.Language).Result).Result);

                await parameter.Interaction.DeferAsync();
                await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
                await HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(TCCreateInfo), parameter, message: "/tccreateinfo successfully used");
            }
        }
    }
}
