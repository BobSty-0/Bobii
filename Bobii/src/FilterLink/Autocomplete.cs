using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class AutoComplete
    {
        public static async Task LinkFilterLogUpdateAutoComplete(SocketAutocompleteInteraction interaction)
        {
            
            var guildUser = (IGuildUser)interaction.User;
            var guild = guildUser.Guild;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guild.Id).Result;
            var choicesList = new List<string>();
            var possibleChoices = new string[] { };

            var logId = EntityFramework.FilterLinkLogsHelper.GetFilterLinkLogChannelID(guild.Id).Result;
            if (logId == 0)
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C029", language).Result };
            }
            else
            {
                foreach (var channel in guild.GetTextChannelsAsync().Result)
                {
                    if (channel.Id == logId)
                    {
                        continue;
                    }
                    choicesList.Add($"{channel.Name} - ID: {channel.Id}");
                }

                if (choicesList.Count == 0)
                {
                    possibleChoices = new string[] { Bobii.Helper.GetCaption("C030", language).Result };
                }
                else
                {
                    possibleChoices = choicesList.ToArray();
                }


                if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
                {
                    possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
                }
            }

            await Bobii.Helper.RespondToAutocomplete(interaction, possibleChoices);
        }

        public static async Task LinkFilterLogSetAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var guild = guildUser.Guild;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guild.Id).Result;
            var choicesList = new List<string>();
            var possibleChoices = new string[] { };
            foreach (var channel in guild.GetTextChannelsAsync().Result)
            {
                choicesList.Add($"{channel.Name} - ID: {channel.Id}");
            }

            if (choicesList.Count == 0)
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C030", language).Result };
            }
            else
            {
                possibleChoices = choicesList.ToArray();
            }


            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(interaction, possibleChoices);
        }

        public static async Task DeleteLinkAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var name = string.Empty;
            foreach (var option in interaction.Data.Options)
            {
                if (option.Name == "name")
                {
                    name = option.Value.ToString();
                }
            }

            var possibleChoices = FilterLink.EntityFramework.FilterLinkOptionsHelper.GetAllOptionsLinksFromGuild(name, guildUser.Guild.Id).Result;

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C031", language).Result };
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(interaction, possibleChoices);
        }

        public static async Task DeleteNameAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = FilterLink.EntityFramework.FilterLinkOptionsHelper.GetAllOptionsFromGuildOrderByBezeichnung(guildUser.Guild.Id).Result;

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C032", language).Result };
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(interaction, possibleChoices);
        }

        public static async Task CreateAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = FilterLink.EntityFramework.FilterLinkOptionsHelper.GetAllOptionsFromGuildOrderByBezeichnung(guildUser.Guild.Id).Result;

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C033", language).Result };
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(interaction, possibleChoices);
        }

        public static async Task AddAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = FilterLink.Helper.GetFilterLinksOfGuild(guildUser.GuildId).Result;

            if (possibleChoices.Count() == 0)
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C034", language).Result };
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(interaction, possibleChoices);
        }

        public static async Task RemoveAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (IGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var filterLinksOfGuild = EntityFramework.FilterLinksGuildHelper.GetLinks(guildUser.GuildId).Result;
            var choicesList = new List<string>();
            foreach (var filterlink in filterLinksOfGuild)
            {
                choicesList.Add(filterlink.bezeichnung.Trim());
            }

            var possibleChoices = new string[] { };
            if (choicesList.Count == 0)
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C035", language).Result };
            }
            else
            {
                possibleChoices = choicesList.ToArray();
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(interaction, possibleChoices);
        }
    }
}
