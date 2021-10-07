using Bobii.src.DBStuff.Tables;
using Bobii.src.Entities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class SlashCommands
    {
        #region Info
        public static async Task FLInfo(SlashCommandParameter parameter)
        {
            //inks = 1 / user = 2
            var linkoruser = int.Parse(Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString());

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkInfo"))
            {
                return;
            }

            if (linkoruser == 1)
            {
                await parameter.Interaction.RespondAsync("", new Embed[] { TextChannel.TextChannel.CreateFilterLinkLinkWhitelistInfoEmbed(parameter.Interaction, parameter.GuildID) });
            }
            else
            {
                await parameter.Interaction.RespondAsync("", new Embed[] { TextChannel.TextChannel.CreateFilterLinkUserWhitelistInfoEmbed(parameter.Interaction, parameter.GuildID) });
            }
        }
        #endregion

        #region Utility
        public static async Task FLSet(SlashCommandParameter parameter)
        {
            var state = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkSet"))
            {
                return;
            }

            if (state == "2")
            {
                if (!filterlink.IsFilterLinkActive(parameter.GuildID))
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Filter link is already inactive", "Already inactive!") }, ephemeral: true);
                    Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already inactive");
                    return;
                }
                try
                {
                    filterlink.DeactivateFilterLink(parameter.GuildID);

                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"I wont filter links anymore from now on!\nTo reactivate filter link use:\n`/flset`", "Filter link deactivated!") });
                    Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | State: active | /flset successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"State of filter link could not be set.", "Error!") }, ephemeral: true);
                    Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to set state | {ex.Message}");
                    return;
                }
            }
            else
            {
                if (filterlink.IsFilterLinkActive(parameter.GuildID))
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Filter link is already active", "Already active!") }, ephemeral: true);
                    Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already active");
                    return;
                }
                try
                {
                    filterlink.ActivateFilterLink(parameter.GuildID);

                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"I will from now on watch out for links!\nIf you want to whitelist specific links for excample YouTube links you can use:\n`/flladd`\nIf you want to add a user to the whitelist so that he can use links without restriction, then you can use:\n`/fluadd`", "Filter link activated!") });
                    Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | State: active | /flset successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"State of filter link could not be set.", "Error!") }, ephemeral: true);
                    Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to set state | {ex.Message}");
                    return;
                }
            }
        }

        #region log
        public static async Task LogSet(SlashCommandParameter parameter)
        {
            var channelId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            if (!channelId.StartsWith("<#"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Make sure to use #channel-name for the parameter <channel>\nYou can do that by simply typing # followed by the channel name", "Wrong input!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong channel input");
                return;
            }
            channelId = channelId.Replace("<", "");
            channelId = channelId.Replace(">", "");
            channelId = channelId.Replace("#", "");

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkLogSet") ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, channelId, parameter.Guild, "FilterLinkLogSet", true))
            {
                return;
            }

            if (filterlinklogs.DoesALogChannelExist(parameter.GuildID))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You already set a log channel: <#{filterlinklogs.GetFilterLinkLogChannelID(parameter.GuildID)}>", "Already set!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLinkLog already set");
                return;
            }

            try
            {
                filterlinklogs.SetFilterLinkLog(parameter.GuildID, ulong.Parse(channelId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The channel <#{channelId}> will now show all messages which will be deleted by Bobii", "Log successfully set") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | /logset successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel could not be set", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogSet | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | Failed to set log channel | {ex.Message}");
                return;
            }
        }

        public static async Task LogUpdate(SlashCommandParameter parameter)
        {
            var channelId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();

            if (!channelId.StartsWith("<#"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Make sure to use #channel-name for the parameter <channel>\nYou can do that by simply typing # followed by the channel name", "Wrong input!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong channel input");
                return;
            }
            channelId = channelId.Replace("<", "");
            channelId = channelId.Replace(">", "");
            channelId = channelId.Replace("#", "");

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkLogUpdate") ||
                Bobii.CheckDatas.CheckDiscordChannelID(parameter.Interaction, channelId, parameter.Guild, "FilterLinkLogUpdate", true))
            {
                return;
            }

            if (!filterlinklogs.DoesALogChannelExist(parameter.GuildID))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have a log channel yet, you can set a log channel by using:\n`/logset`", "No log channel yet!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| No filterlink log channel to update");
                return;
            }

            try
            {
                filterlinklogs.UpdateFilterLinkLog(parameter.GuildID, ulong.Parse(channelId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel was sucessfully changed to <#{channelId}>", "Successfully updated") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | /logupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel could not be updated", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogUpdate | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Channel: {channelId} | Failed to update log channel | {ex.Message}");
                return;
            }
        }

        public static async Task LogRemove(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkLogRemove"))
            {
                return;
            }

            if (!filterlinklogs.DoesALogChannelExist(parameter.GuildID))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"You dont have a log channel yet, you can set a log channel by using:\n`/logset`", "No log channel yet!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| No filterlink log channel to update");
                return;
            }

            try
            {
                filterlinklogs.RemoveFilterLinkLog(parameter.GuildID);

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel was successfully removed", "Successfully removed") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | /logremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The log channel could not be removed", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkLogRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Failed to remove log channel | {ex.Message}");
                return;
            }
        }
        #endregion

        #region User
        public static async Task FLUAdd(SlashCommandParameter parameter)
        {
            //TODO Check for valid Id (also if user is on this server) -> replace the old functions
            var userId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            if (!userId.StartsWith("<@") && !userId.EndsWith(">"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Make shure to use @User for the parameter <user>", "Wrong input!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong User input");
                return;
            }
            userId = userId.Replace("<", "");
            userId = userId.Replace(">", "");
            userId = userId.Replace("@", "");
            userId = userId.Replace("!", "");

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkWhitelistUserAdd") ||
                Bobii.CheckDatas.CheckUserID(parameter.Interaction, userId, parameter.Guild, "FilterLinkWhitelistUserAdd"))
            {
                return;
            }

            if (filterlinkuserguild.IsUserOnWhitelistInGuild(parameter.GuildID, ulong.Parse(userId)))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is already whitelisted", "Already on whitelist!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already whitelisted");
                return;
            }

            try
            {
                var filterLinkActiveText = "";
                if (!filterlink.IsFilterLinkActive(parameter.GuildID))
                {
                    filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
                }

                filterlinkuserguild.AddWhiteListUserToGuild(parameter.GuildID, ulong.Parse(userId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The user  <@{userId}> is now on the whitelist.{filterLinkActiveText}", "User successfully added") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {userId} | /fluadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"User could not be added to the whitelist", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | User: {userId} | Failed to add user to whitelist | {ex.Message}");
                return;
            }
        }

        public static async Task FLURemove(SlashCommandParameter parameter)
        {
            //TODO Check for valid Id (also if user is on this server) -> replace the old functions
            var userId = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            if (!userId.StartsWith("<@") && !userId.EndsWith(">"))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Make shure to use @User for the parameter <user>", "Wrong input!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| Wrong User input");
                return;
            }
            userId = userId.Replace("<", "");
            userId = userId.Replace(">", "");
            userId = userId.Replace("@", "");
            userId = userId.Replace("!", "");

            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkWhitelistUserAdd") ||
                Bobii.CheckDatas.CheckUserID(parameter.Interaction, userId, parameter.Guild, "FilterLinkWhitelistUserAdd"))
            {
                return;
            }

            if (!filterlinkuserguild.IsUserOnWhitelistInGuild(parameter.GuildID, ulong.Parse(userId)))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is not on the whitelisted", "Not on whitelist!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already whitelisted");
                return;
            }

            try
            {
                filterlinkuserguild.RemoveWhiteListUserFromGuild(parameter.GuildID, ulong.Parse(userId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"The user <@{userId}> is no longer on the whitelist", "User successfully removed") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {userId} | /fluremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"User could not be added to the whitelist", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistUserRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | User: {userId} | Failed to remove user from whitelist | {ex.Message}");
                return;
            }
        }
        #endregion

        #region Links
        public static async Task FLLAdd(SlashCommandParameter parameter)
        {
            var link = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkWhitelistAdd"))
            {
                return;
            }
            if (filterlinksguild.IsFilterlinkAllowedInGuild(parameter.GuildID, link))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Links of **{link}** are already whitelisted", "Already on whitelist!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink already whitelisted");
                return;
            }

            try
            {
                var filterLinkActiveText = "";
                if (!filterlink.IsFilterLinkActive(parameter.GuildID))
                {
                    filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
                }
                filterlinksguild.AddToGuild(parameter.GuildID, link);

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"**{link}** links are now on the whitelist. {filterLinkActiveText}", "Link successfully added") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | /flwadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Link could not be added to the whitelist", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistAdd | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | Failed to add link to whitelist | {ex.Message}");
                return;
            }
        }

        public static async Task FLLRemove(SlashCommandParameter parameter)
        {
            var link = Handler.SlashCommandHandlingService.GetOptions(parameter.SlashCommand.Data.Options)[0].Value.ToString();
            if (Bobii.CheckDatas.CheckUserPermission(parameter.Interaction, parameter.Guild, parameter.GuildUser, parameter.SlashCommand, "FilterLinkWhitelistRemove"))
            {
                return;
            }
            if (!filterlinksguild.IsFilterlinkAllowedInGuild(parameter.GuildID, link))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Links of **{link}** are not whitelisted yet", "Not on whitelist!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser}| FilterLink is not on whitelist");
                return;
            }

            try
            {
                filterlinksguild.RemoveFromGuild(parameter.GuildID, link);

                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"**{link}** links are no longer on the whitelist", "Link successfully removed") });
                Handler.SlashCommandHandlingService.WriteToConsol($"Information: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | /flwremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { TextChannel.TextChannel.CreateEmbed(parameter.Interaction, $"Link could not be removed from the whitelist", "Error!") }, ephemeral: true);
                Handler.SlashCommandHandlingService.WriteToConsol($"Error: {parameter.Guild.Name} | Task: FilterLinkWhitelistRemove | Guild: {parameter.GuildID} | User: {parameter.GuildUser} | Link: {link} | Failed to remove link from the whitelist | {ex.Message}");
                return;
            }
        }
        #endregion
        #endregion
    }
}
