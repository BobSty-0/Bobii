using Bobii.src.Helper;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class AutoComplete
    {
        public static async Task TempKickAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (SocketGuildUser)interaction.User;
            var guild = (SocketGuild)guildUser.Guild;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guild.Id).Result;

            var possibleChoices = new string[] { };
            var choicesList = new List<string>();
            var userInVoice = guildUser.VoiceState.Value.VoiceChannel.ConnectedUsers.Where(u => u.Id != guildUser.Id);

            foreach (var user in userInVoice)
            {
                var userFormat = string.Empty;
                if (user.Nickname != null)
                {
                    userFormat = $"{user.Nickname} ID: {user.Id}";
                }
                else
                {
                    userFormat = $"{user.Username} ID: {user.Id}";
                }
                choicesList.Add(userFormat);
            }
            if (choicesList.Count == 0)
            {
                possibleChoices = new string[] { GeneralHelper.GetContent("C093", language).Result };
            }
            else
            {
                possibleChoices = choicesList.ToArray();
            }

            await GeneralHelper.RespondToAutocomplete(interaction, possibleChoices);
        }

        public static async Task TempOwnerAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (SocketGuildUser)interaction.User;
            var guild = (SocketGuild)guildUser.Guild;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guild.Id).Result;

            var possibleChoices = new string[] { };
            var choicesList = new List<string>();
            var userInVoice = guildUser.VoiceState.Value.VoiceChannel.ConnectedUsers.Where(u => u.Id != guildUser.Id);

            foreach (var user in userInVoice)
            {
                if (user.IsBot)
                {
                    continue;
                }
                var userFormat = string.Empty;
                if (user.Nickname != null)
                {
                    userFormat = $"{user.Nickname} ID: {user.Id}";
                }
                else
                {
                    userFormat = $"{user.Username} ID: {user.Id}";
                }
                choicesList.Add(userFormat);
            }
            if (choicesList.Count == 0)
            {
                possibleChoices = new string[] { GeneralHelper.GetContent("C094", language).Result };
            }
            else
            {
                possibleChoices = choicesList.ToArray();
            }

            await GeneralHelper.RespondToAutocomplete(interaction, possibleChoices);
        }

        //Übernommen
        public static async Task AddAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (SocketGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = new string[] { };

            var guild = (SocketGuild)guildUser.Guild;

            var choicesList = new List<string>();

            var createTempChannels = EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild);

            foreach (var channel in guild.VoiceChannels)
            {
                var createTempChannel = createTempChannels.Result.Where(ch => ch.createchannelid == channel.Id).FirstOrDefault();
                if (createTempChannel != null)
                {
                    continue;
                }
                choicesList.Add($"{channel.Name} - ID: {channel.Id}");
            }

            if (choicesList.Count == 0)
            {
                possibleChoices = new string[]  { GeneralHelper.GetContent("C095", language).Result };
            }
            else
            {
                possibleChoices = choicesList.ToArray();
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { GeneralHelper.GetCaption("C028", language).Result };
            }

            await GeneralHelper.RespondToAutocomplete(interaction, possibleChoices);
        }

        public static async Task UpdateRemoveAutoComplete(SocketAutocompleteInteraction interaction)
        {
            var guildUser = (SocketGuildUser)interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = new string[] { };

            var guild = (SocketGuild)guildUser.Guild;

            var createTempChannels = EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild).Result;
            var choicesList = new List<string>();

            foreach (var createTempChannel in createTempChannels)
            {
                var voiceChannel = guild.GetVoiceChannel(createTempChannel.createchannelid);
                if (voiceChannel == null)
                {
                    continue;
                }
                else
                {
                    choicesList.Add($"{voiceChannel.Name} - ID: {voiceChannel.Id}");
                }

            }

            if (choicesList.Count == 0)
            {
                possibleChoices = new string[] { GeneralHelper.GetContent("C096", language).Result };
            }
            else
            {
                possibleChoices = choicesList.ToArray();
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { GeneralHelper.GetCaption("C028", language).Result };
            }

            await GeneralHelper.RespondToAutocomplete(interaction, possibleChoices);
        }
    }
}
