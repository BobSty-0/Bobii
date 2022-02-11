using Bobii.src.Entities;
using Discord;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Bobii.src.Bobii;

namespace Bobii.src.FilterLink
{
    class SlashCommands
    {
        #region Info
        public static async Task FLGuildInfo(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(FLGuildInfo)).Result)
            {
                return;
            }
            await parameter.Interaction.RespondAsync("", new Embed[] { FilterLink.Helper.CreateFLGuildInfo(parameter.Interaction, parameter.GuildID).Result });
            await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLGuildInfo), parameter, message: $"/flguildinfo successfully used");
        }

        public static async Task FLInfo(SlashCommandParameter parameter)
        {
            //inks = 1 / user = 2
            var linkoruser = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "usersorlinks").Result.Integer;

            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(FLInfo)).Result)
            {
                return;
            }

            if (linkoruser == 1)
            {
                await parameter.Interaction.RespondAsync("", new Embed[] { FilterLink.Helper.CreateFilterLinkLinkWhitelistInfoEmbed(parameter).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLInfo), parameter, message: $"/flinfo <links> successfully used");
            }
            else
            {
                await parameter.Interaction.RespondAsync("", new Embed[] { FilterLink.Helper.CreateFilterLinkUserWhitelistInfoEmbed(parameter).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLInfo), parameter, message: $"/flinfo <users> successfully used");
            }
        }
        #endregion

        #region Utility
        public static async Task FLCreate(SlashCommandParameter parameter)
        {
            var name = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "name").Result.String;
            var link = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "link").Result.String;

            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(FLCreate)).Result)
            {
                return;
            }

            if (name.Contains(Bobii.Helper.GetCaption("C033", parameter.Language).Result.ToLower()))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                    Bobii.Helper.GetContent("C036", parameter.Language).Result, 
                    Bobii.Helper.GetCaption("C036", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLCreate), parameter, link: link, message: $"no name successtions yet");
                return;
            }

            if (CheckDatas.CheckLinkFormat(parameter, link, nameof(FLCreate)).Result)
            {
                return;
            }

            name = name.ToLower();
            link = link.Link2LinkOptions();

            if (CheckDatas.CheckStringLength(parameter, name, 20, "the filter-link name", nameof(FLCreate)).Result ||
            CheckDatas.CheckStringLength(parameter, link, 40, "the filter-link", nameof(FLCreate)).Result ||
            CheckDatas.CheckIfFilterLinkOptionAlreadyExists(parameter, name, link, nameof(FLCreate)).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterLinkOptionsHelper.AddLinkOption(name, link, parameter.GuildID);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C037", parameter.Language).Result, link, name),
                    Bobii.Helper.GetCaption("C037", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLCreate), parameter, link: link, message: $"/flcreate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C038", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLCreate), parameter, message: $"Failed to create filter link option", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task FLDelete(SlashCommandParameter parameter)
        {
            var name = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "name").Result.String;
            var link = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "link").Result.String;

            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(FLDelete)).Result)
            {
                return;
            }

            if (name.Contains(Bobii.Helper.GetCaption("C032", parameter.Language).Result.ToLower()))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C039", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C036", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLDelete), parameter, link: link, message: $"no name successtions yet");
                return;
            }

            if (link == Bobii.Helper.GetCaption("C031", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C039", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C036", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLDelete), parameter, link: link, message: $"no name successtions yet");
                return;
            }

            link = link.Replace("https://", "");

            if (Bobii.CheckDatas.CheckStringLength(parameter, name, 20, "the filter-link name", nameof(FLDelete)).Result ||
                Bobii.CheckDatas.CheckStringLength(parameter, link, 40, "the filter-link", nameof(FLDelete)).Result ||
                Bobii.CheckDatas.CheckIfFilterLinkOptionExists(parameter, name, link, nameof(FLDelete)).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterLinkOptionsHelper.DeleteLinkOption(name, link, parameter.GuildID);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C040", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C040", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLDelete), parameter, link: link, message: $"/fldelete successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C041", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLDelete), parameter, message: $"Failed to delete filter link option", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task FLSet(SlashCommandParameter parameter)
        {
            var state = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "state").Result.Integer;

            //Check for valid input + permission
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(FLSet)).Result)
            {
                return;
            }

            if (state == 2)
            {
                if (!EntityFramework.FilterlLinksHelper.FilterLinkAktive(parameter.GuildID).Result)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Bobii.Helper.GetContent("C042", parameter.Language).Result,
                        Bobii.Helper.GetCaption("C042", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLSet), parameter, message: $"FilterLink already inactive");
                    return;
                }
                try
                {
                    await EntityFramework.FilterlLinksHelper.DeactivateFilterLink(parameter.GuildID);

                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Bobii.Helper.GetContent("C043", parameter.Language).Result,
                        Bobii.Helper.GetCaption("C043", parameter.Language).Result).Result });
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLSet), parameter, filterLinkState: "inactive", message: $"/flset successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Bobii.Helper.GetContent("C044", parameter.Language).Result,
                        Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLSet), parameter, message: $"Failed to set state", exceptionMessage: ex.Message);
                    return;
                }
            }
            else
            {
                if (EntityFramework.FilterlLinksHelper.FilterLinkAktive(parameter.GuildID).Result)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Bobii.Helper.GetContent("C045", parameter.Language).Result,
                        Bobii.Helper.GetCaption("C045", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLSet), parameter, message: $"FilterLink already active");
                    return;
                }
                try
                {
                    await EntityFramework.FilterlLinksHelper.ActivateFilterLink(parameter.GuildID);

                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Bobii.Helper.GetContent("C046", parameter.Language).Result,
                        Bobii.Helper.GetCaption("C046", parameter.Language).Result).Result });
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLSet), parameter, filterLinkState: "active", message: $"/flset successfully used");
                }
                catch (Exception ex)
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                        Bobii.Helper.GetContent("C047", parameter.Language).Result,
                        Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                    await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLSet), parameter, message: $"Failed to set state", exceptionMessage: ex.Message);
                    return;
                }
            }
        }

        #region log
        public static async Task LogSet(SlashCommandParameter parameter)
        {
            var channelId = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "channel").Result.String;
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(LogSet)).Result)
            {
                return;
            }

            if (channelId.Contains(Bobii.Helper.GetCaption("C030", parameter.Language).Result.ToLower()))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C048", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C030", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(LogSet), parameter, message: $"Could not find any text channel");
                return;
            }

            channelId = channelId.Split(' ')[channelId.Split().Count() - 1];

            if (Bobii.CheckDatas.CheckDiscordChannelIDFormat(parameter, channelId, nameof(LogSet), true).Result)
            {
                return;
            }

            if (EntityFramework.FilterLinkLogsHelper.DoesALogChannelExist(parameter.GuildID).Result)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C049", parameter.Language).Result, EntityFramework.FilterLinkLogsHelper.GetFilterLinkLogChannelID(parameter.GuildID).Result),
                    Bobii.Helper.GetCaption("C049", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(LogSet), parameter, message: $"FilterLinkLog already set");
                return;
            }

            try
            {
                await EntityFramework.FilterLinkLogsHelper.SetFilterLinkLogChannel(parameter.GuildID, ulong.Parse(channelId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C050", parameter.Language).Result, channelId),
                    Bobii.Helper.GetCaption("C050", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(LogSet), parameter, logID: ulong.Parse(channelId), message: $"/logset successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C051", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(LogSet), parameter, logID: ulong.Parse(channelId),
                    message: $"Failed to set log channel", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task LogUpdate(SlashCommandParameter parameter)
        {
            var channelId = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "channel").Result.String;
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(LogUpdate)).Result)
            {
                return;
            }

            if (channelId.Contains(Bobii.Helper.GetCaption("C030", parameter.Language).Result.ToLower()))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C048", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C030", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(LogUpdate), parameter, message: $"Could not find any text channel");
                return;
            }

            if (channelId.Contains(Bobii.Helper.GetCaption("C029", parameter.Language).Result.ToLower()))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C052", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C052", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(LogUpdate), parameter, message: "No log channel yet");
                return;
            }

            channelId = channelId.Split(' ')[channelId.Split().Count() - 1];

            if (CheckDatas.CheckDiscordChannelIDFormat(parameter, channelId, nameof(LogUpdate), true).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterLinkLogsHelper.UpdateFilterLinkLogChannel(parameter.GuildID, ulong.Parse(channelId));

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C053", parameter.Language).Result, channelId),
                    Bobii.Helper.GetCaption("C053", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(LogUpdate), parameter, logID: ulong.Parse(channelId), 
                    message: $"/logupdate successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C054", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(LogUpdate), parameter, logID: ulong.Parse(channelId), 
                    message: $"Failed to update log channel", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task LogRemove(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(LogRemove)).Result ||
                CheckDatas.DoesALogChannelExist(parameter, nameof(LogRemove)).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterLinkLogsHelper.RemoveFilterLinkLogChannel(parameter.GuildID);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C055", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C055", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(LogRemove), parameter, message: $"/logremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C056", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(LogRemove), parameter, message: $"Failed to remove log channel", exceptionMessage: ex.Message);
                return;
            }
        }
        #endregion

        #region User
        public static async Task FLUAdd(SlashCommandParameter parameter)
        {
            //TODO Check for valid Id (also if user is on this server) -> replace the old functions
            var user = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "user").Result.IUser;

            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(FLUAdd)).Result ||
                Bobii.CheckDatas.IsUserAlreadyOnWhiteList(parameter, user.Id, nameof(FLUAdd)).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterLinkUserGuildHelper.AddWhiteListUserToGuild(parameter.GuildID, user.Id);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C058", parameter.Language).Result, user.Id),
                    Bobii.Helper.GetCaption("C058", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLUAdd), parameter, message: $"/fluadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C059", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLUAdd), parameter, message: $"Failed to add user to whitelist", exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task FLURemove(SlashCommandParameter parameter)
        {
            var user = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "user").Result.IUser;

            if (Bobii.CheckDatas.CheckUserPermission(parameter, "FilterLinkWhitelistUserAdd").Result ||
                Bobii.CheckDatas.IsUserOnWhiteList(parameter, user.Id, nameof(FLURemove)).Result) 
            {
                return;
            }

            try
            {
                await EntityFramework.FilterLinkUserGuildHelper.RemoveWhiteListUserFromGuild(parameter.GuildID, user.Id);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C061", parameter.Language).Result, user.Id),
                    Bobii.Helper.GetCaption("C061", parameter.Language).Result).Result }) ;
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLURemove), parameter, message: $"/fluremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction, 
                    Bobii.Helper.GetContent("C062", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLURemove), parameter, message: $"Failed to remove user from whitelist"
                    , exceptionMessage: ex.Message);
                return;
            }
        }
        #endregion

        #region Links
        public static async Task FLLAdd(SlashCommandParameter parameter)
        {
            var link = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "link").Result.String;

            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(FLLAdd)).Result ||
                Bobii.CheckDatas.CheckIfFilterLinkIsAlreadyWhitelisted(parameter, link, nameof(FLLAdd)).Result)
            {
                return;
            }

            var options = await FilterLink.Helper.GetFilterLinksOfGuild(parameter.GuildID);

            if (!options.Contains(link))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C064", parameter.Language).Result, link),
                    Bobii.Helper.GetCaption("C064", parameter.Language).Result).Result }, ephemeral: true) ;
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLLAdd), parameter, message: $"User tryed to use a choice which is not provided");
                return;
            }

            if (link == Bobii.Helper.GetCaption("C034", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C065", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C065", parameter.Language).Result).Result },
                    ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLLAdd), parameter, message: $"No more links to add");
                return;
            }

            try
            {
                await EntityFramework.FilterLinksGuildHelper.AddToGuild(parameter.GuildID, link);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C066", parameter.Language).Result, link),
                    Bobii.Helper.GetCaption("C066", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLLAdd), parameter, link: link, message: $"/flwadd successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C067", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLLAdd), parameter, link: link, message: $"Failed to add link to whitelist",
                    exceptionMessage: ex.Message);
                return;
            }
        }

        public static async Task FLLRemove(SlashCommandParameter parameter)
        {
            var link = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "link").Result.String;
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(FLLRemove)).Result)
            {
                return;
            }

            var options = EntityFramework.FilterLinksGuildHelper.GetLinks(parameter.GuildID).Result;
            if (!options.Any(row => row.bezeichnung.Contains(link)))
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C068", parameter.Language).Result, link),
                    Bobii.Helper.GetCaption("C068", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLLRemove), parameter, message: $"User tryed to use a choice which is not provided");
                return;
            }

            if (link == Bobii.Helper.GetCaption("C035", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C069", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C069", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", true, nameof(FLLRemove), parameter, message: $"No more links to remove");
                return;
            }

            if (Bobii.CheckDatas.CheckIfFilterLinkOptionIsWhitelisted(parameter, link, nameof(FLLRemove)).Result)
            {
                return;
            }

            try
            {
                await EntityFramework.FilterLinksGuildHelper.RemoveFromGuild(parameter.GuildID, link);

                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    String.Format(Bobii.Helper.GetContent("C071", parameter.Language).Result, link),
                    Bobii.Helper.GetCaption("C071", parameter.Language).Result).Result });
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLLRemove), parameter, link: link, message: $"/flwremove successfully used");
            }
            catch (Exception ex)
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C072", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C038", parameter.Language).Result).Result }, ephemeral: true);
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SlashComms", false, nameof(FLLRemove), parameter, link: link, message: $"Failed to remove link from the whitelist", exceptionMessage: ex.Message);
                return;
            }
        }
        #endregion
        #endregion
    }
}
