using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    class ShlashCommands
    {
        public static async Task SlashCommandHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parsedArg = (SocketSlashCommand)interaction;
            var parsedGuildUser = (SocketGuildUser)parsedArg.User;

            switch (parsedArg.Data.Name)
            {
                case "tempinfo":
                    await interaction.RespondAsync("", false, TempVoiceChannel.TempVoiceChannel.CreateVoiceChatInfoEmbed(parsedGuildUser.Guild.Id.ToString(), client));
                    break;
                case "help":
                    await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateHelpInfoSlash(parsedGuildUser.Guild.Id.ToString(), interaction, client));
                    break;
            }
        }
    }
}
