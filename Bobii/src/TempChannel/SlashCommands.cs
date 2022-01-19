using Bobii.src.Entities;
using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class SlashCommands
    {
        #region Info
        public static async Task TCInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "tcinfo").Result)
            {
                return;
            }
            await parameter.Interaction.RespondAsync("", new Embed[] { TempChannel.Helper.CreateVoiceChatInfoEmbed(parameter.Guild,
                parameter.Client, parameter.Interaction) });
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TCInfo", parameter, message: "/tcinfo successfully used");
        }

        public static async Task TCCreateInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData,
                "TCCreateInfo").Result)
            {
                return;
            }
            await parameter.Interaction.Channel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(parameter.Interaction,
                Helper.HelpEditTempChannelInfoPart(parameter.Client.Rest.GetGlobalApplicationCommands().Result, true).Result,
                "Use this commands to edit your temporary voice channels:").Result);

            await parameter.Interaction.DeferAsync();
            await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TCCreateInfo", parameter, message: "/tccreateinfo successfully used");
        }
        #endregion

        #region Utility
        public static async Task TCAdd(SlashCommandParameter parameter)
        {
            var channelSize = string.Empty;
            var textChannel = string.Empty;
            bool textChannelb = false;
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Bobii is not " +
                    $"able to find any channels of your guild which you could add as temporary voice channels. This is usually because all " +
                    $"the voice channels of this guild are already added as create-temp-channels or Bobii is missing permissions to get a list " +
                    $"of all voicechannels.", "Could not find any channels!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCAdd", parameter, message: "Could not find any channels");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You " +
                    $"dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:" +
                    $"\n`Administrator`\n`Manage Server`!", "Missing permissions!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCAdd", parameter, message: "Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];
            var name = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();

            if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Count > 2)
            {
                if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[2].Name == "channelsize")
                {
                    channelSize = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[2].Value.ToString();
                    if (Bobii.CheckDatas.CheckIfInputIsNumber(parameter.Interaction, parameter.Guild, parameter.GuildUser, channelSize, "the channel size", "TempAdd").Result)
                    {
                        return;
                    }
                }
                else
                {
                    textChannel = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[2].Value.ToString();
                    if (textChannel == "on")
                    {
                        textChannelb = true;
                    }
                }
            }

            if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Count > 3)
            {
                if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[2].Name == "channelsize")
                {
                    channelSize = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[2].Value.ToString();
                    textChannel = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[3].Value.ToString();

                }
                else
                {
                    textChannel = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[2].Value.ToString();
                    channelSize = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[3].Value.ToString();
                }

                if (Bobii.CheckDatas.CheckIfInputIsNumber(parameter.Interaction, parameter.Guild, parameter.GuildUser, channelSize, "the channel size", "TempAdd").Result)
                {
                    return;
                }
                if (textChannel == "on")
                {
                    textChannelb = true;
                }
            }

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempAdd").Result ||
                Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter.Interaction, createChannelID, parameter.Guild, "TempAdd", true, parameter.Language).Result ||
                Bobii.CheckDatas.CheckIfIDBelongsToVoiceChannel(parameter.Interaction, createChannelID, "TempAdd", parameter.Guild, parameter.Language).Result ||
                Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDExists(parameter.Interaction, createChannelID, parameter.Guild, "TempAdd", parameter.Language).Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, createChannelID, parameter.Guild, name, "TempAdd", 50, true, parameter.Language).Result)

            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            name = name.Replace("'", "’");

            try
            {
                await EntityFramework.CreateTempChannelsHelper.AddCC(parameter.GuildID, name, ulong.Parse(createChannelID), channelSize, textChannelb);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The create-temp-channel **'" +
                    $"{parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully added by **{parameter.GuildUser.Username}**",
                    "Create-temp-channel sucessfully added!").Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TCAdd", parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "/tcadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Create-temp-channel could " +
                    "not be added", "Error!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCAdd", parameter, createChannelID: ulong.Parse(createChannelID),
                    message: "Failed to add CreateTempChannel", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task TCUpdate(SlashCommandParameter parameter)
        {
            string voiceNameNew = String.Empty;
            string voiceSizeNew = String.Empty;
            string textChannelNew = String.Empty;
            bool textChannelNewb = false;
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "create-temp-channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont " +
                    $"have any create-temp-channels yet!\nYou can add a create-temp-channel by using:\n`/tcadd`", "No create-temp-channels yet!").Result },
                    ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCUpdate", parameter, message: "Could not find any channels");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have enough " +
                    $"permissions to use this command.\nMake sure you have one of the named permissions below:\n`Administrator`\n`Manage Server`!",
                    "Missing permissions!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCUpdate", parameter, message: "Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];

            // Only the create-temp-channel was choosen
            if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Count == 1)
            {
                return;
            }

            if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "newtempchannelname").FirstOrDefault() != null)
            {
                voiceNameNew = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "newtempchannelname").First().Value.ToString();
            }

            if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "newtempchannelsize").FirstOrDefault() != null)
            {
                voiceSizeNew = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "newtempchannelsize").First().Value.ToString();
            }

            if (Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "textchannel").FirstOrDefault() != null)
            {
                textChannelNew = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result.Where(e => e.Name == "textchannel").First().Value.ToString();
                if (textChannelNew == "on")
                {
                    textChannelNewb = true;
                }
            }

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempChangeName").Result ||
                Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter.Interaction, createChannelID, parameter.Guild, "TempChangeName", true, parameter.Language).Result ||
                Bobii.CheckDatas.CheckIfCreateTempChannelWithGivenIDAlreadyExists(parameter.Interaction, createChannelID, parameter.Guild, "TempChangeName", parameter.Language).Result)
            {
                return;
            }

            if (voiceNameNew != "")
            {
                if (Bobii.CheckDatas.CheckNameLength(parameter.Interaction, createChannelID, parameter.Guild, voiceNameNew, "TempChangeName", 50, true, parameter.Language).Result)
                {
                    return;
                }

                try
                {
                    await EntityFramework.CreateTempChannelsHelper.ChangeTempChannelName(voiceNameNew, ulong.Parse(createChannelID));
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"Temp-channel successfully updated", "Successfully changed!").Result });
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TCUpdate", parameter, createChannelID: ulong.Parse(createChannelID),
                        message: "/tcupdate successfully used (name)");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Temp-channel name could not be changed", "Error!").Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCUpdate", parameter, createChannelID: ulong.Parse(createChannelID),
                        message: "Failed to update TempChannelName", exceptionMessage: ex.Message);
                    return;
                }
            }

            if (voiceSizeNew != "")
            {
                if (Bobii.CheckDatas.CheckIfInputIsNumber(parameter.Interaction, parameter.Guild, parameter.GuildUser, voiceSizeNew, "the channel size", "TempAdd").Result)
                {
                    return;
                }

                try
                {
                    await EntityFramework.CreateTempChannelsHelper.ChangeTempChannelSize(int.Parse(voiceSizeNew), ulong.Parse(createChannelID));
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"Temp-channel successfully updated", "Successfully changed!").Result });
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TCUpdate", parameter, createChannelID: ulong.Parse(createChannelID),
                        message: "/tcupdate successfully used (size)");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Temp-channel size could not be changed", "Error!").Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCUpdate", parameter, createChannelID: ulong.Parse(createChannelID),
                        message: "Failed to update TempChannelSize", exceptionMessage: ex.Message);
                    return;
                }
            }

            if (textChannelNew != "")
            {
                try
                {
                    await EntityFramework.CreateTempChannelsHelper.ChangeTextChannel(textChannelNewb, ulong.Parse(createChannelID));
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"Temp-channel successfully updated", "Successfully changed!").Result });
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TCUpdate", parameter, createChannelID: ulong.Parse(createChannelID),
                        message: "/tcupdate successfully used (text-channel)");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Text-channel could not be changed", "Error!").Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCUpdate", parameter, createChannelID: ulong.Parse(createChannelID),
                        message: "Failed to update TextChannel", exceptionMessage: ex.Message);
                    return;
                }
            }
            await Task.CompletedTask;
        }

        public static async Task TCRemove(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "create-temp-channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"You dont have any create-temp-channels yet!\nYou can add a create-temp-channel by using:\n`/tcadd`", "No create-temp-channels yet!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCRemove", parameter, message: "Could not find any channels");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"You dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:" +
                    $"\n`Administrator`\n`Manage Server`!", "Missing permissions!").Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, "TCRemove", parameter, message: "Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempRemove").Result ||
                Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter.Interaction, createChannelID, parameter.Guild, "TempRemove", true, parameter.Language).Result ||
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
            try
            {
                if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempName").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempName").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempChannel").Result ||
                Bobii.CheckDatas.CheckStringLength(parameter.Interaction, parameter.Guild, newName, 50, "the channel name", "TempName").Result)
                {
                    return;
                }

                await parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.Name = newName);
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
                    await parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = null);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        $"The temp-channel size was successfully changed to **unlimited** because the given number was bigger then 99 (max user limit to set)",
                        "Size sucessfully changed!").Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, "TempSize", parameter, tempChannelID: parameter.GuildUser.VoiceChannel.Id,
                        message: "/tempsize successfully used");
                }
                else
                {
                    await parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = int.Parse(newSize));
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

            if (Bobii.CheckDatas.CheckUserID(parameter.Interaction, parameter.Guild, parameter.GuildUser, user, parameter.Client, "TempBlock").Result)
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

                await voiceChannel.AddPermissionOverwriteAsync(parameter.Client.GetUserAsync(userId).Result, newPermissionOverride);

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

            if (Bobii.CheckDatas.CheckUserID(parameter.Interaction, parameter.Guild, parameter.GuildUser, user, parameter.Client, "TempUnBlock").Result)
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
                var newPermissionOverride = new OverwritePermissions().Modify(connect: PermValue.Allow, viewChannel: PermValue.Allow);
                var voiceChannel = parameter.GuildUser.VoiceChannel;

                await voiceChannel.AddPermissionOverwriteAsync(parameter.Client.GetUserAsync(userId).Result, newPermissionOverride);

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
