using Bobii.src.Entities;
using Discord;
using System;
using System.Collections.Generic;
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
            await parameter.Interaction.RespondAsync("", new Embed[] { TempChannel.Helper.CreateVoiceChatInfoEmbed(parameter.Guild, parameter.Client, parameter.Interaction) });
            await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempInfo | Guild: {parameter.GuildID} | /tcinfo successfully used");
        }

        public static async Task TCCreateInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TCCreateInfo").Result)
            {
                return;
            }
            await parameter.Interaction.Channel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(parameter.Interaction, Helper.HelpEditTempChannelInfoPart(parameter.Client.Rest.GetGlobalApplicationCommands().Result).Result, "All my commands to edit temp-channels:").Result);

            await parameter.Interaction.DeferAsync();
            await parameter.Interaction.GetOriginalResponseAsync().Result.DeleteAsync();
            await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TCCreateInfo | Guild: {parameter.GuildID} | /tccreateinfo successfully used");
        }
        #endregion

        #region Utility
        public static async Task TCAdd(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Bobii is not able to find any channels of your guild which you could add as temporary voice channels. This is usually because all the voice channels of this guild are already added as create-temp-channels or Bobii is missing permissions to get a list of all voicechannels.", "Could not find any channels!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Could not find any channels");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:\n`Administrator`\n`Manage Server`!", "Missing permissions!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];
            var name = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempAdd").Result ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempAdd", true).Result ||
                Bobii.CheckDatas.CheckIfVoiceID(parameter.Interaction, createChannelID, "TempAdd", parameter.Guild).Result ||
                Bobii.CheckDatas.CheckDoubleCreateTempChannel(parameter.Interaction, createChannelID, parameter.Guild, "TempAdd").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, createChannelID, parameter.Guild, name, "TempAdd", 50, true).Result)
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            name = name.Replace("'", "’");

            try
            {
                await EntityFramework.CreateTempChannelsHelper.AddCC(parameter.GuildID, name, ulong.Parse(createChannelID));
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The create-temp-channel **'{parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully added by **{parameter.GuildUser.Username}**", "Create-temp-channel sucessfully added!").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Create-temp-channel could not be added", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to add CreateTempChannel | {ex.Message}");
                return;
            }
        }

        public static async Task TCUpdate(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "create-temp-channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have any create-temp-channels yet!\nYou can add a create-temp-channel by using:\n`/tcadd`", "No create-temp-channels yet!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Could not find any channels");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:\n`Administrator`\n`Manage Server`!", "Missing permissions!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];
            var voiceNameNew = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[1].Value.ToString();

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempChangeName").Result ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempChangeName", true).Result ||
                Bobii.CheckDatas.CheckIfCreateTempChannelExists(parameter.Interaction, createChannelID, parameter.Guild, "TempChangeName").Result ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, createChannelID, parameter.Guild, voiceNameNew, "TempChangeName", 50, true).Result)
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            voiceNameNew = voiceNameNew.Replace("'", "’");

            try
            {
                await EntityFramework.CreateTempChannelsHelper.ChangeTempChannelName(voiceNameNew, ulong.Parse(createChannelID));
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Temp-channel name successfully changed to: **'{voiceNameNew}'**", "Name successfully changed!").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempChangeName | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Temp-channel name could not be changed", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempChangeName | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to update TempChannelName | {ex.Message}");
                return;
            }
            await Task.CompletedTask;
        }

        public static async Task TCRemove(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "create-temp-channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have any create-temp-channels yet!\nYou can add a create-temp-channel by using:\n`/tcadd`", "No create-temp-channels yet!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Could not find any channels");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:\n`Administrator`\n`Manage Server`!", "Missing permissions!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempRemove").Result ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempRemove", true).Result ||
                Bobii.CheckDatas.CheckIfCreateTempChannelExists(parameter.Interaction, createChannelID, parameter.Guild, "TempRemove").Result)
            {
                return;
            }

            try
            {
                await EntityFramework.CreateTempChannelsHelper.RemoveCC(parameter.GuildID.ToString(), ulong.Parse(createChannelID));
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The create-temp-channel **'{parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully removed by **{parameter.GuildUser.Username}**", "Create-temp-channel successfully removed!").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempRemove | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Create-temp-channel could not be removed", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempRemove | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to remove CreateTempChannel | {ex.Message}");
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

                await parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.Name = newName);
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The temp-channel name was successfully changed to **{newName}**", "Name sucessfully changed!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempName | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /tempname successfully used");
            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempName | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | Failed to change temp-channel name | {ex.Message}");
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Temp-channel name could not be changed!", "Error!").Result }, ephemeral: true);
                return;
            }
        }

        public static async Task TempSize(SlashCommandParameter parameter)
        {
            var newSize = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            if (Bobii.CheckDatas.CheckIfUserInVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempName").Result ||
                Bobii.CheckDatas.CheckIfUserInTempVoice(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempName").Result ||
                Bobii.CheckDatas.CheckIfUserIsOwnerOfTempChannel(parameter.Interaction, parameter.Guild, parameter.GuildUser, "TempChannel").Result ||
                Bobii.CheckDatas.CheckIfInputIsNumber(parameter.Interaction, parameter.Guild, parameter.GuildUser, newSize, "size", "TempChannel").Result)
            {
                return;
            }

            try
            {
                if (int.Parse(newSize) > 99)
                {
                    await parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = null);
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The temp-channel size was successfully changed to **unlimited** because the given number was bigger then 99 (max user limit to set)", "Size sucessfully changed!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempSize | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /tempsize successfully used");
                }
                else
                {
                    await parameter.GuildUser.VoiceChannel.ModifyAsync(channel => channel.UserLimit = int.Parse(newSize));
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The temp-channel size was successfully changed to **{newSize}**", "Size sucessfully changed!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempSize | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /tempsize successfully used");
                }


            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempSize | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | Failed to change temp-channel size | {ex.Message}");
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
            user = user.Replace("<@!", "");
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
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The temp-channel owner was successfully changed!\nNew owner: <@{userId}>", "Owner sucessfully changed!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempSize | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /tempowner successfully used");
            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempOwner | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | Failed to change temp-channel owner | {ex.Message}");
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
            user = user.Replace("<@!", "");
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
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"User <@{toBeKickedUser.Id}> successfully removed from the temp-channel", "User sucessfully removed!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempKick | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /tempkick successfully used");

            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempKick | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | Failed to kick temp-channel user | {ex.Message}");
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


                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Temp-channel successfully locked", "Successfully locked!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempLock | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /templock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempLock | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | Failed to lock temp-channel  | {ex.Message}");
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
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempUnLock | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /tempunlock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempUnLock | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | Failed to unlock temp-channel  | {ex.Message}");
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
            user = user.Replace("<@!", "");
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
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempBlock | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /tempblock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempBlock | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | Failed to block user from temp-channel  | {ex.Message}");
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
            user = user.Replace("<@!", "");
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
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempUnBlock | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | /tempunblock successfully used");
            }
            catch (Exception ex)
            {
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempUnBlock | Guild: {parameter.GuildID} | ChannelID: {parameter.GuildUser.VoiceChannel.Id} | User: {parameter.GuildUser} | Failed to unblock user from temp-channel  | {ex.Message}");
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "User could not be unblocked from Temp-channel!", "Error!").Result }, ephemeral: true);
            }
        }
        #endregion
    }
}
