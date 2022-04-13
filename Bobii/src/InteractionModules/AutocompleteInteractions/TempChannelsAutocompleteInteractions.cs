using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.AutocompleteInteractions
{
    class TempChannelsAutocompleteInteractions : InteractionModuleBase<SocketInteractionContext<SocketAutocompleteInteraction>>
    {
        [AutocompleteCommand("createvoicechannel", "add")]
        public async Task Add()
        {
            var guildUser = (SocketGuildUser)Context.Interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = new string[] { };

            var guild = (SocketGuild)guildUser.Guild;

            var choicesList = new List<string>();

            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild);

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
                possibleChoices = new string[] { Bobii.Helper.GetContent("C095", language).Result };
            }
            else
            {
                possibleChoices = choicesList.ToArray();
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(Context.Interaction, possibleChoices);
        }

        [AutocompleteCommand("createvoicechannel_name", "createtempchannel")]
        public async Task UpdateName()
        {
            var guildUser = (SocketGuildUser)Context.Interaction.User;
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildUser.Guild.Id).Result;
            var possibleChoices = new string[] { };

            var guild = (SocketGuild)guildUser.Guild;

            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild).Result;
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
                possibleChoices = new string[] { Bobii.Helper.GetContent("C096", language).Result };
            }
            else
            {
                possibleChoices = choicesList.ToArray();
            }

            if (!(guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild))
            {
                possibleChoices = new string[] { Bobii.Helper.GetCaption("C028", language).Result };
            }

            await Bobii.Helper.RespondToAutocomplete(Context.Interaction, possibleChoices);
        }
    }
}
