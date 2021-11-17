using Bobii.src.Entities;
using Discord;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class SlashCommands
    {
        #region Info
        public static async Task FLInfo(SlashCommandParameter parameter)
        {
            //inks = 1 / user = 2
            var linkoruser = int.Parse(Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString());

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkInfo").Result)
            {
                return;
            }

            if (linkoruser == 1)
            {
                await parameter.Interaction.RespondAsync("", new Embed[] { FilterLink.Helper.CreateFilterLinkLinkWhitelistInfoEmbed(parameter.Interaction, parameter.GuildID).Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FLInfo | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | /flinfo <links> successfully used");
            }
            else
            {
                await parameter.Interaction.RespondAsync("", new Embed[] { FilterLink.Helper.CreateFilterLinkUserWhitelistInfoEmbed(parameter.Interaction, parameter.GuildID).Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FLInfo | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | /flinfo <user> successfully used");
            }
        }
        #endregion

        #region Utility
        public static async Task FLSet(SlashCommandParameter parameter)
        {
            var state = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkSet").Result)
            {
                return;
            }

            if (state == "2")
            {
                if (!EntityFramework.FilterlLinksHelper.FilterLinkAktive(parameter.GuildID).Result)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Filter link is already inactive", "Already inactive!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already inactive");
                    return;
                }
                try
                {
                    await EntityFramework.FilterlLinksHelper.DeactivateFilterLink(parameter.GuildID);

                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"I wont filter links anymore from now on!\nTo reactivate filter link use:\n`/flset`", "Filter link deactivated!").Result });
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | State: active | /flset successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"State of filter link could not be set.", "Error!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to set state | {ex.Message}");
                    return;
                }
            }
            else
            {
                if (EntityFramework.FilterlLinksHelper.FilterLinkAktive(parameter.GuildID).Result)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Filter link is already active", "Already active!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already active");
                    return;
                }
                try
                {
                    await EntityFramework.FilterlLinksHelper.ActivateFilterLink(parameter.GuildID);

                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"I will from now on watch out for links!\nIf you want to whitelist specific links for excample YouTube links you can use:\n`/flladd`\nIf you want to add a user to the whitelist so that he can use links without restriction, then you can use:\n`/fluadd`", "Filter link activated!").Result });
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | State: active | /flset successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"State of filter link could not be set.", "Error!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to set state | {ex.Message}");
                    return;
                }
            }
        }

        #region log
        public static async Task LogSet(SlashCommandParameter parameter)
        {
            var channelId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            if (!channelId.StartsWith("<#"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Make sure to use #channel-name for the parameter <channel>\nYou can do that by simply typing # followed by the channel name", "Wrong input!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong channel input");
                return;
            }
            channelId = channelId.Replace("<", "");
            channelId = channelId.Replace(">", "");
            channelId = channelId.Replace("#", "");

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkLogSet").Result ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, channelId, parameter.Guild, "FilterLinkLogSet", true).Result)
            {
                return;
            }

            if (EntityFramework.FilterLinkLogsHelper.DoesALogChannelExist(parameter.GuildID).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You already set a log channel: <#{EntityFramework.FilterLinkLogsHelper.GetFilterLinkLogChannelID(parameter.GuildID).Result}>", "Already set!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLinkLog already set");
                return;
            }

            try
            {
                await EntityFramework.FilterLinkLogsHelper.SetFilterLinkLogChannel(parameter.GuildID, ulong.Parse(channelId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The channel <#{channelId}> will now show all messages which will be deleted by Bobii", "Log successfully set").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | /logset successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The log channel could not be set", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | Failed to set log channel | {ex.Message}");
                return;
            }
        }

        public static async Task LogUpdate(SlashCommandParameter parameter)
        {
            var channelId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();

            if (!channelId.StartsWith("<#"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Make sure to use #channel-name for the parameter <channel>\nYou can do that by simply typing # followed by the channel name", "Wrong input!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong channel input");
                return;
            }
            channelId = channelId.Replace("<", "");
            channelId = channelId.Replace(">", "");
            channelId = channelId.Replace("#", "");

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkLogUpdate").Result ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, channelId, parameter.Guild, "FilterLinkLogUpdate", true).Result)
            {
                return;
            }

            if (!EntityFramework.FilterLinkLogsHelper.DoesALogChannelExist(parameter.GuildID).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have a log channel yet, you can set a log channel by using:\n`/logset`", "No log channel yet!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| No filterlink log channel to update");
                return;
            }

            try
            {
                await EntityFramework.FilterLinkLogsHelper.UpdateFilterLinkLogChannel(parameter.GuildID, ulong.Parse(channelId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The log channel was sucessfully changed to <#{channelId}>", "Successfully updated").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | /logupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The log channel could not be updated", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | Failed to update log channel | {ex.Message}");
                return;
            }
        }

        public static async Task LogRemove(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkLogRemove").Result)
            {
                return;
            }

            if (!EntityFramework.FilterLinkLogsHelper.DoesALogChannelExist(parameter.GuildID).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"You dont have a log channel yet, you can set a log channel by using:\n`/logset`", "No log channel yet!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| No filterlink log channel to update");
                return;
            }

            try
            {
                await EntityFramework.FilterLinkLogsHelper.RemoveFilterLinkLogChannel(parameter.GuildID);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The log channel was successfully removed", "Successfully removed").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | /logremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The log channel could not be removed", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to remove log channel | {ex.Message}");
                return;
            }
        }
        #endregion

        #region User
        public static async Task FLUAdd(SlashCommandParameter parameter)
        {
            //TODO Check for valid Id (also if user is on this server) -> replace the old functions
            var userId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            if (!userId.StartsWith("<@") && !userId.EndsWith(">"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Make shure to use @User for the parameter <user>", "Wrong input!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong User input");
                return;
            }
            userId = userId.Replace("<", "");
            userId = userId.Replace(">", "");
            userId = userId.Replace("@", "");
            userId = userId.Replace("!", "");

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkWhitelistUserAdd").Result ||
                Bobii.CheckDatas.CheckUserID(parameter.Interaction, userId, parameter.Guild, "FilterLinkWhitelistUserAdd").Result)
            {
                return;
            }

            if (EntityFramework.FilterLinkUserGuildHelper.IsUserOnWhitelistInGuild(parameter.GuildID, ulong.Parse(userId)).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is already whitelisted", "Already on whitelist!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already whitelisted");
                return;
            }

            try
            {
                var filterLinkActiveText = "";
                if (!EntityFramework.FilterlLinksHelper.FilterLinkAktive(parameter.GuildID).Result)
                {
                    filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
                }

                await EntityFramework.FilterLinkUserGuildHelper.AddWhiteListUserToGuild(parameter.GuildID, ulong.Parse(userId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The user  <@{userId}> is now on the whitelist.{filterLinkActiveText}", "User successfully added").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {userId} | /fluadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"User could not be added to the whitelist", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | User: {userId} | Failed to add user to whitelist | {ex.Message}");
                return;
            }
        }

        public static async Task FLURemove(SlashCommandParameter parameter)
        {
            //TODO Check for valid Id (also if user is on this server) -> replace the old functions
            var userId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            if (!userId.StartsWith("<@") && !userId.EndsWith(">"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Make shure to use @User for the parameter <user>", "Wrong input!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong User input");
                return;
            }
            userId = userId.Replace("<", "");
            userId = userId.Replace(">", "");
            userId = userId.Replace("@", "");
            userId = userId.Replace("!", "");

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkWhitelistUserAdd").Result ||
                Bobii.CheckDatas.CheckUserID(parameter.Interaction, userId, parameter.Guild, "FilterLinkWhitelistUserAdd").Result)
            {
                return;
            }

            if (!EntityFramework.FilterLinkUserGuildHelper.IsUserOnWhitelistInGuild(parameter.GuildID, ulong.Parse(userId)).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is not on the whitelisted", "Not on whitelist!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already whitelisted");
                return;
            }

            try
            {
                await EntityFramework.FilterLinkUserGuildHelper.RemoveWhiteListUserFromGuild(parameter.GuildID, ulong.Parse(userId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is no longer on the whitelist", "User successfully removed").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {userId} | /fluremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"User could not be added to the whitelist", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | User: {userId} | Failed to remove user from whitelist | {ex.Message}");
                return;
            }
        }
        #endregion

        #region Links
        public static async Task FLLAdd(SlashCommandParameter parameter)
        {
            var link = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkWhitelistAdd").Result)
            {
                return;
            }

            if (EntityFramework.FilterLinksGuildHelper.IsFilterlinkAllowedInGuild(parameter.GuildID, link).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Links of **{link}** are already whitelisted", "Already on whitelist!").Result }, ephemeral: true);
                return;
            }

            var options = await FilterLink.Helper.GetFilterLinksOfGuild(parameter.GuildID);
            if (!options.Contains(link))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The link **{link}** is not a choice.\nIf you think this link should be provided as choice, feel free to direct message <@776028262740393985> and I will add it!", "The given link is not provided as choice!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | User tryed to use a choice which is not provided");
                return;
            }

            if (link == "already all links added")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"There are no more links to add.\nIf you miss a choice which you need please direct message <@776028262740393985> and I will add it!", "No more links to add!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| No more links to add");
                return;
            }

            try
            {
                var filterLinkActiveText = "";
                if (!EntityFramework.FilterlLinksHelper.FilterLinkAktive(parameter.GuildID).Result)
                {
                    filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
                }
                await EntityFramework.FilterLinksGuildHelper.AddToGuild(parameter.GuildID, link);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"**{link}** links are now on the whitelist. {filterLinkActiveText}", "Link successfully added").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | /flwadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Link could not be added to the whitelist", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | Failed to add link to whitelist | {ex.Message}");
                return;
            }
        }

        public static async Task FLLRemove(SlashCommandParameter parameter)
        {
            var link = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommandData.Options).Result[0].Value.ToString();
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommandData, "FilterLinkWhitelistRemove").Result)
            {
                return;
            }

            var options = EntityFramework.FilterLinksGuildHelper.GetLinks(parameter.GuildID).Result;
            if (!options.Any(row => row.bezeichnung.Contains(link)))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"The link **{link}** is not a choice.\nYou can only remove links which are provided as choice!", "The given link is not provided as choice!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | User tryed to use a choice which is not provided");
                return;
            }

            if (link == "no links to remove yet")
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"There are no links to remove yet.\nYou can add links by using:\n`/flladd`", "No links to remove!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| No more links to add");
                return;
            }

            if (!EntityFramework.FilterLinksGuildHelper.IsFilterlinkAllowedInGuild(parameter.GuildID, link).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Links of **{link}** are not whitelisted yet", "Not on whitelist!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink is not on whitelist");
                return;
            }

            try
            {
                await EntityFramework.FilterLinksGuildHelper.RemoveFromGuild(parameter.GuildID, link);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"**{link}** links are no longer on the whitelist", "Link successfully removed").Result });
                await Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | /flwremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, $"Link could not be removed from the whitelist", "Error!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | Failed to remove link from the whitelist | {ex.Message}");
                return;
            }
        }
        #endregion
        #endregion
    }
}
