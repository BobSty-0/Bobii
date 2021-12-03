using Bobii.src.Entities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Watch2Gether
{
    class SlashCommands
    {
        public static async Task W2GStart(SlashCommandParameter parameter)
        {
            var nameAndID = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString().Split(" ");
            if (nameAndID[nameAndID.Count() - 1] == "channels")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                    $"Bobii is not able to find any channels of your guild which you could add as temporary voice channels. This is usually because all the voice channels of this guild are already added as create-temp-channels or Bobii is missing permissions to get a list of all voicechannels.", 
                    "Could not find any channels!").Result }, ephemeral: true);
                await Bobii.Helper.WriteToConsol("SlashComms", true, "W2GStart", parameter,
                    message: "Could not find any channels (Autocomplete)");
                return;
            }

            if (nameAndID[nameAndID.Count() - 1] == "rights")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    $"You dont have enough permissions to use this command.\nMake sure you have one of the named permissions below:\n`Administrator`\n`Manage Server`!", 
                    "Missing permissions!").Result }, ephemeral: true);
                await Bobii.Helper.WriteToConsol("SlashComms", true, "W2GStart", parameter,
                    message: "Not enought rights (Autocomplete)");
                return;
            }
            var voiceChannelID = nameAndID[nameAndID.Count() - 1];

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "W2GStart").Result ||
                Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter.Interaction, voiceChannelID, parameter.Guild, "W2GStart", true).Result ||
                Bobii.CheckDatas.CheckIfIDBelongsToVoiceChannel(parameter.Interaction, voiceChannelID, "W2GStart", parameter.Guild).Result ||
                Bobii.CheckDatas.CheckIfChannelIDBelongsToACreateTempChannel(parameter.Interaction, voiceChannelID, parameter.Guild, "W2GStart").Result)
            {
                return;
            }

            try
            {
                var voiceChannel = parameter.Guild.GetVoiceChannel(ulong.Parse(voiceChannelID));
                var invite = voiceChannel.CreateInviteToApplicationAsync(880218394199220334, null).Result;
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                    $"Link to invite the YouTube application was successfully created for **{parameter.Guild.GetChannel(ulong.Parse(voiceChannelID)).Name}**\nClick here to join:\n{invite.Url}", "Sucessfully created!").Result });
                await Bobii.Helper.WriteToConsol("SlashComms", false, "W2GStart", parameter, message: "/w2gstart successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, "Youtube application could not be added", "Error!").Result }, ephemeral: true);
                await Bobii.Helper.WriteToConsol("SlashComms", true, "W2GStart", parameter, message: "Failed to add youtube application", exceptionMessage: ex.Message);
                return;
            }
        }
    }
}
