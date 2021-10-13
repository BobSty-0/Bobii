using Bobii.src.DBStuff.Tables;
using Bobii.src.Entities;
using Discord;
using System;
using System.Collections.Generic;
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
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "tcinfo"))
            {
                return;
            }
            await parameter.Interaction.RespondAsync("", new Embed[] { TempChannel.Helper.CreateVoiceChatInfoEmbed(parameter.Guild, parameter.Client, parameter.Interaction) });
            Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempInfo | Guild: {parameter.GuildID} | /tcinfo successfully used");
        }
        #endregion

        #region Utility
        public static async Task TCAdd(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options)[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count()-1] == "channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Bobii is not able to find any channels of your guild which you could add as temporary voice channels. This is usually because all the voice channels of this guild are already added as create-temp-channels or Bobii is missing permissions to get a list of all voicechannels.", "Could not find any channels!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Could not find any channels");
                return;           
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:\n`Administrator`\n`Manage Server`!", "Missing permissions!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];
            var name = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options)[1].Value.ToString();

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempAdd") ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempAdd", true) ||
                Bobii.CheckDatas.CheckIfVoiceID(parameter.Interaction, createChannelID, "TempAdd", parameter.Guild) ||
                Bobii.CheckDatas.CheckDoubleCreateTempChannel(parameter.Interaction, createChannelID, parameter.Guild, "TempAdd") ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, createChannelID, parameter.Guild, name, "TempAdd", 50, true))
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            name = name.Replace("'", "’");

            try
            {
                createtempchannels.AddCC(parameter.GuildID, name, createChannelID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The create-temp-channel **'{parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully added by **{parameter.GuildUser.Username}**", "Create-temp-channel sucessfully added!") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "Create-temp-channel could not be added", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to add CreateTempChannel | {ex.Message}");
                return;
            }
        }

        public static async Task TCUpdate(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options)[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "create-temp-channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have any create-temp-channels yet!\nYou can add a create-temp-channel by using:\n`/tcadd`", "No create-temp-channels yet!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Could not find any channels");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:\n`Administrator`\n`Manage Server`!", "Missing permissions!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];
            var voiceNameNew = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options)[1].Value.ToString();

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempChangeName") ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempChangeName", true) ||
                Bobii.CheckDatas.CheckIfCreateTempChannelExists(parameter.Interaction, createChannelID, parameter.Guild, "TempChangeName") ||
                Bobii.CheckDatas.CheckNameLength(parameter.Interaction, createChannelID, parameter.Guild, voiceNameNew, "TempChangeName", 50, true))
            {
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            voiceNameNew = voiceNameNew.Replace("'", "’");

            try
            {
                createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Temp-channel name succesfully changed to: **'{voiceNameNew}'**", "Name successfully changed!") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempChangeName | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "Temp-channel name could not be changed", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempChangeName | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to update TempChannelName | {ex.Message}");
                return;
            }
            createtempchannels.ChangeTempChannelName(voiceNameNew, createChannelID);
            await Task.CompletedTask;
        }

        public static async Task TCRemove(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options)[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "create-temp-channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have any create-temp-channels yet!\nYou can add a create-temp-channel by using:\n`/tcadd`", "No create-temp-channels yet!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Could not find any channels");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:\n`Administrator`\n`Manage Server`!", "Missing permissions!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Not enought rights");
                return;
            }
            var createChannelID = nameAndID[nameAndID.Count() - 1];

            //Checking for valid input and Permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "TempRemove") ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, createChannelID, parameter.Guild, "TempRemove", true) ||
                Bobii.CheckDatas.CheckIfCreateTempChannelExists(parameter.Interaction, createChannelID, parameter.Guild, "TempRemove"))
            {
                return;
            }

            try
            {
                createtempchannels.RemoveCC(parameter.GuildID.ToString(), createChannelID);
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The create-temp-channel **'{parameter.Guild.GetChannel(ulong.Parse(createChannelID)).Name}'** was sucessfully removed by **{parameter.GuildUser.Username}**", "Create-temp-channel successfully removed!") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: TempRemove | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | /tcremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, "Create-temp-channel could not be removed", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: TempRemove | Guild: {parameter.GuildID} | CreateChannelID: {createChannelID} | User: {parameter.GuildUser} | Failed to remove CreateTempChannel | {ex.Message}");
                return;
            }
        }
        #endregion
    }
}
