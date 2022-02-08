using Bobii.src.Entities;
using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class SlashCommands
    {
        #region Info
        public static async Task TCInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCInfo)).Result)
            {
                return;
            }
            await parameter.Interaction.RespondAsync("", new Embed[] { TempChannel.Helper.CreateVoiceChatInfoEmbed(parameter) });
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(TCInfo), parameter, message: "/tcinfo successfully used");
        }

        public static async Task TCCreateInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCCreateInfo)).Result)
            {
                return;
            }
            await parameter.Interaction.Channel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(parameter.Interaction,
                Helper.HelpEditTempChannelInfoPart(parameter.Client.Rest.GetGlobalApplicationCommands().Result, parameter.GuildID, true).Result,
                Bobii.Helper.GetContent("C106", parameter.Language).Result).Result);

            await parameter.Interaction.DeferAsync();
            await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(TCCreateInfo), parameter, message: "/tccreateinfo successfully used");
        }
        #endregion

        #region Utility
        public static async Task TCAdd(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCAdd)).Result)
            {
                return;
            }
            bool textChannelb = false;

            var createVoiceChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "createvoicechannel").Result.String;
            var tempChannelName = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "tempchannelname").Result.String;
            var channelSize = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "channelsize").Result.Integer;
            var textChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "textchannel").Result.String;

            var nameAndID = createVoiceChannel.Split(" ");


            if (createVoiceChannel == Bobii.Helper.GetContent("C095", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C107", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C107", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(TCAdd), parameter, message: "Could not find any channels");
                return;
            }

            var createChannelID = nameAndID[nameAndID.Count() - 1];

            if (textChannel != null)
            {
                if (textChannel == "on")
                {
                    textChannelb = true;
                }
            }

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, nameof(TCAdd), true).Result ||
                Bobii.CheckDatas.CheckIfIDBelongsToVoiceChannel(parameter.Interaction, createChannelID, nameof(TCAdd), parameter.Guild, parameter.Language).Result ||
                Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDExists(parameter.Interaction, createChannelID, parameter.Guild, nameof(TCAdd), parameter.Language).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter, createChannelID, tempChannelName, nameof(TCAdd), 50, true).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.CreateTempChannelsHelper.AddCC(parameter.GuildID, tempChannelName, ulong.Parse(createChannelID), channelSize, textChannelb);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    string.Format(Bobii.Helper.GetContent("C108", parameter.Language).Result, parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name, parameter.GuildUser.Username),
                    Bobii.Helper.GetCaption("C108", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(TCAdd), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "/tcadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C109", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(TCAdd), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "Failed to add CreateTempChannel", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task TCUpdate(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCUpdate)).Result)
            {
                return;
            }

            bool textChannelNewb = false;
            var sb = new StringBuilder();
            var createVoiceChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "createvoicechannel").Result.String;
            var newTempChannelName = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newtempchannelname").Result.String;
            var newChannelSize = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "newtempchannelsize").Result.Integer;
            var newTextChannel = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "textchannel").Result.String;

            var nameAndID = createVoiceChannel.Split(" ");
            var createChannelID = nameAndID[nameAndID.Count() - 1];

            if (createVoiceChannel == Bobii.Helper.GetContent("C096", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C110", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C110", parameter.Language).Result).Result },
                    ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(TCUpdate), parameter, message: "Could not find any channels");
                return;
            }

            // Only the create-temp-channel was choosen
            if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Count == 1)
            {
                return;
            }

            if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, nameof(TCUpdate), true).Result ||
                 Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter.Interaction, createChannelID, parameter.Guild, nameof(TCUpdate), parameter.Language).Result)
            {
                return;
            }

            try
            {
                if (newTempChannelName != null)
                {
                    if (Bobii.CheckDatas.CheckNameLength(parameter, createChannelID, newTempChannelName, nameof(TCUpdate), 50, true).Result)
                    {
                        return;
                    }

                    await EntityFramework.CreateTempChannelsHelper.ChangeTempChannelName(newTempChannelName, ulong.Parse(createChannelID));
                    sb.AppendLine();
                    sb.AppendLine(string.Format(Bobii.Helper.GetContent("C111", parameter.Language).Result, newTempChannelName));
                }

                if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "newtempchannelsize").FirstOrDefault() != null)
                {
                    await EntityFramework.CreateTempChannelsHelper.ChangeTempChannelSize(newChannelSize, ulong.Parse(createChannelID));
                    sb.AppendLine();
                    sb.AppendLine(string.Format(Bobii.Helper.GetContent("C112", parameter.Language).Result, newChannelSize));
                }

                if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "textchannel").FirstOrDefault() != null)
                {
                    if (newTextChannel == "on")
                    {
                        textChannelNewb = true;
                    }
                    await EntityFramework.CreateTempChannelsHelper.ChangeTextChannel(textChannelNewb, ulong.Parse(createChannelID));
                    sb.AppendLine();
                    sb.AppendLine(string.Format(Bobii.Helper.GetContent("C113", parameter.Language).Result, newTextChannel));
                }


                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    sb.ToString(), 
                    Bobii.Helper.GetCaption("C111", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(TCUpdate), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "/tcupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C114", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(TCUpdate), parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "Failed to update create-temp-channel", exceptionMessage: ex.Message);
                return;
            }

            await Task.CompletedTask;
        }

        public static async Task TCRemove(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString().Split(" ");
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(TCRemove)).Result)
            {
                return;
            }
            if (nameAndID[nameAndID.Count() - 1] == "create-temp-channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"You dont have any create-temp-channels yet!\nYou can add a create-temp-channel by using:\n`/tcadd`", "No create-temp-channels yet!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCRemove", parameter, message: "Could not find any channels");
                return;
            }

            var createChannelID = nameAndID[nameAndID.Count() - 1];

            if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, createChannelID, "TempRemove", true).Result ||
                Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter.Interaction, createChannelID, parameter.Guild, "TempRemove", parameter.Language).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.CreateTempChannelsHelper.RemoveCC(parameter.GuildID.ToString(), ulong.Parse(createChannelID));
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"The create-temp-channel **'{parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully removed by **{parameter.GuildUser.Username}**", "Create-temp-channel successfully removed!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TCRemove", parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "/tcremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    "Create-temp-channel could not be removed", "Error!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCRemove", parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "Failed to remove CreateTempChannel", exceptionMessage: ex.Message);
                return;
            }
        }
        #endregion

        #region EditChannel

        public static async Task TempName(SlashCommandParameter parameter)
        {
            var newName = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            try
            {
                if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempName").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempName").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempChannel").Result ||
                Bobii.CheckDatas.CheckStringLength(parameter.Interaction, parameter.Guild, newName, 50, "the channel name", "TempName").Result)
                {
                    return;
                }

                try
                {
                    _ = Task.Run(async () => parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.Name = newName));
                }
                catch (Exception ex)
                {
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempName", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Rate limited", exceptionMessage: ex.Message);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    "Bobii has been rate limited, you cannot change the voice channel name more than 2 times in 5mins!", "Error!").Result }, ephemeral: true);
                    return;
                }
                var tempChannel = EntityFramework.TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;
                if (tempChannel.textchannelid != 0)
                {
                    var textChannel = parameter.Client.Guilds
                        .SelectMany(g => g.Channels)
                        .FirstOrDefault(c => c.Id == tempChannel.textchannelid);

                    if (textChannel != null)
                    {
                        await textChannel.ModifyAsync(channel => channel.Name = newName);
                    }
                }

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"The temp-channel name was successfully changed to **{newName}**", "Name sucessfully changed!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempName", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempname successfully used");


            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempName", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel name", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    "Temp-channel name could not be changed!", "Error!").Result }, ephemeral: true);
                return;
            }
        }

        public static async Task TempSize(SlashCommandParameter parameter)
        {
            var newSize = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempSize").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempSize").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempSize").Result ||
                Bobii.CheckDatas.CheckIfInputIsNumber(parameter.Interaction, parameter.Guild, parameter.GuildUser, newSize, "size", "TempSize").Result)
            {
                return;
            }

            try
            {
                if (int.Parse(newSize) > 99)
                {
                    _ = parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = null);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        $"The temp-channel size was successfully changed to **unlimited** because the given number was bigger then 99 (max user limit to set)",
                        "Size sucessfully changed!").Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempSize", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempsize successfully used");
                }
                else
                {
                    _ = Task.Run(async () => parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = int.Parse(newSize)));
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        $"The temp-channel size was successfully changed to **{newSize}**", "Size sucessfully changed!").Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempSize", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempsize successfully used");
                }


            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempSize", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel size", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Temp-channel size could not be changed!", "Error!").Result }, ephemeral: true);
                return;
            }
        }

        public static async Task TempOwner(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            if (user == Bobii.Helper.GetContent("C094", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"I could not find any user to give the owner to.\nYou are not able to give the owner to a Bot or to yourselfe!", "Could not find any users!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempOwner", parameter, message: "Could not find any users");
                return;
            }

            user = user.Split(' ')[user.Split(' ').Count() - 1];

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckUserID(parameter.Interaction, parameter.Guild, parameter.GuildUser, user, parameter.Client, "TempOwner").Result)
            {
                return;
            }
            user = user.Replace("<@", "");
            user = user.Replace("!", "");
            user = user.Replace(">", "");
            var userId = ulong.Parse(user);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempOwner").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempOwner").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempOwner").Result ||
                Bobii.CheckDatas.CheckIfUserInSameTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, userId, parameter.Client, "TempOwner").Result)
            {
                return;
            }

            try
            {
                var tempChannel = EntityFramework.TempChannelsHelper.GetTempChannel(parameter.GuildUser.VoiceChannel.Id).Result;

                var currentOwner = parameter.Client.GetUser(tempChannel.channelownerid.Value);
                var newOwner = parameter.Client.GetUser(ulong.Parse(user));

                if (newOwner.IsBot)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        $"You can not pass the owner to a Bot, please choose a user!", "Cannot give owner to an Bot!").Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempOwner", new Entities.SlashCommandParameter() { Guild = parameter.Guild, GuildUser = parameter.GuildUser },
                        message: "User is a Bot");
                    return;
                }

                var voiceChannel = parameter.Client.GetChannel(tempChannel.channelid);
                await Helper.RemoveManageChannelRightsToUserVc(currentOwner, voiceChannel as SocketVoiceChannel);

                if (tempChannel.textchannelid != 0)
                {
                    var textChannel = parameter.Client.Guilds
                        .SelectMany(g => g.Channels)
                        .SingleOrDefault(c => c.Id == tempChannel.textchannelid);
                    await Helper.RemoveManageChannelRightsToUserTc(currentOwner, textChannel as SocketTextChannel);

                    await Helper.GiveManageChannelRightsToUserTc(newOwner, null, textChannel as SocketTextChannel);
                }

                await Helper.GiveManageChannelRightsToUserVc(newOwner, null, voiceChannel as SocketVoiceChannel);


                await EntityFramework.TempChannelsHelper.ChangeOwner(parameter.GuildUser.VoiceChannel.Id, userId);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"The temp-channel owner was successfully changed!\nNew owner: <@{userId}>", "Owner sucessfully changed!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempOwner", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempowner successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempOwner", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to change temp-channel owner", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Temp-channel owner could not be changed!", "Error!").Result }, ephemeral: true);
                return;
            }
        }

        public static async Task TempKick(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            if (user == Bobii.Helper.GetContent("C093", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"I could not find any user to kick out of the channel.", "Could not find any users!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempOwner", parameter, message: "Could not find any users");
                return;
            }

            user = user.Split(' ')[user.Split(' ').Count() - 1];
            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckUserID(parameter.Interaction, parameter.Guild, parameter.GuildUser, user, parameter.Client, "TempKick").Result)
            {
                return;
            }
            user = user.Replace("<@", "");
            user = user.Replace("!", "");
            user = user.Replace(">", "");
            var userId = ulong.Parse(user);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempKick").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempKick").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempKick").Result ||
                Bobii.CheckDatas.CheckIfUserInSameTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, userId, parameter.Client, "TempKick").Result)
            {
                return;
            }

            var usedGuild = parameter.Client.GetGuild(parameter.Guild.Id);

            var toBeKickedUser = usedGuild.GetUser(userId);
            try
            {
                await toBeKickedUser.ModifyAsync(channel => channel.Channel = null);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"User <@{toBeKickedUser.Id}> successfully removed from the temp-channel", "User sucessfully removed!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempKick", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempkick successfully used");

            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempKick", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to kick temp-channel user", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "User could not be kicked!", "Error!").Result }, ephemeral: true);
            }
        }

        public static async Task TempLock(SlashCommandParameter parameter)
        {
            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempLock").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempLock").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempLock").Result)
            {
                return;
            }

            try
            {
                var everyoneRole = parameter.Guild.Roles.Where(role => role.Name == "@everyone").First();
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                var newPermissionOverride = new OverwritePermissions(connect: PermValue.Deny);
                var test = voiceChannel.AddPermissionOverwriteAsync(everyoneRole, newPermissionOverride);


                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"Temp-channel successfully locked", "Successfully locked!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempLock", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/templock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempLock", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to lock temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Temp-channel could not be locked!", "Error!").Result }, ephemeral: true);
            }
        }


        public static async Task TempUnLock(SlashCommandParameter parameter)
        {
            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempLock").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempLock").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempLock").Result)
            {
                return;
            }

            try
            {
                var everyoneRole = parameter.Guild.Roles.Where(role => role.Name == "@everyone").First();
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                var newPermissionOverride = new OverwritePermissions(connect: PermValue.Allow);
                await voiceChannel.AddPermissionOverwriteAsync(everyoneRole, newPermissionOverride);


                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Temp-channel successfully unlocked", "Successfully unlocked!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempUnLock", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunlock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempUnLock", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unlock temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Temp-channel could not be unlocked!", "Error!").Result }, ephemeral: true);
            }
        }

        public static async Task TempBlock(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckUserID(parameter.Interaction, parameter.Guild, parameter.GuildUser, user, parameter.Client, "TempBlock", true).Result)
            {
                return;
            }
            user = user.Replace("<@", "");
            user = user.Replace("!", "");
            user = user.Replace(">", "");
            var userId = ulong.Parse(user);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempBlock").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempBlock").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempBlock").Result ||
                Bobii.CheckDatas.CheckIfUserInGuild(parameter.Interaction, parameter.Guild, parameter.GuildUser, userId, parameter.Client, "TempBlock").Result)
            {
                return;
            }

            try
            {
                var newPermissionOverride = new OverwritePermissions().Modify(connect: PermValue.Deny, viewChannel: PermValue.Deny);
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                _ = voiceChannel.AddPermissionOverwriteAsync(parameter.Client.GetUserAsync(userId).Result, newPermissionOverride);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"User successfully blocked from this temp-channel", "Successfully blocked!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempBlock", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempblock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempBlock", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to block user from temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "User could not be blocked from Temp-channel!", "Error!").Result }, ephemeral: true);
            }
        }

        public static async Task TempUnBlock(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            await Helper.GiveOwnerIfOwnerIDZero(parameter);

            if (Bobii.CheckDatas.CheckUserID(parameter.Interaction, parameter.Guild, parameter.GuildUser, user, parameter.Client, "TempUnBlock", true).Result)
            {
                return;
            }
            user = user.Replace("<@", "");
            user = user.Replace("!", "");
            user = user.Replace(">", "");
            var userId = ulong.Parse(user);

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempUnBlock").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempUnBlock").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempUnBlock").Result ||
                Bobii.CheckDatas.CheckIfUserInGuild(parameter.Interaction, parameter.Guild, parameter.GuildUser, userId, parameter.Client, "TempUnBlock").Result)
            {
                return;
            }

            try
            {
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                await voiceChannel.RemovePermissionOverwriteAsync(parameter.Client.GetUserAsync(userId).Result);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"User successfully unblocked from this temp-channel", "Successfully unblocked!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempUnBlock", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "/tempunblock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TempUnBlock", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                    message: "Failed to unblock user from temp-channel", exceptionMessage: ex.Message);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "User could not be unblocked from Temp-channel!", "Error!").Result }, ephemeral: true);
            }
        }
        #endregion
    }
}
