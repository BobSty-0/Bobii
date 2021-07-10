using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace Bobii.src.TextChannel
{
    class TextChannel
    {
        #region Declaration

        #endregion

        #region Methods

        #endregion

        #region Functions
        //public static string GetAvatarUrl(SocketUser user, ushort size = 1024)
        //{
        //    return user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();
        //}

        public static Embed CreateHelpInfoEmbed(string guildid, SocketInteraction interaction, DiscordSocketClient client)
        {
            var sbTempChannel = new StringBuilder();
            // §TODO 08.07.2021 JG add different command to the help displayed embed
            var sbChannelCommand = new StringBuilder();

            var parsedGuild = GetGuildWithInteraction(interaction);
            var commandList = client.Rest.GetGlobalApplicationCommands();

            sbTempChannel.AppendLine("**__TempChannel commands:__**");

            foreach (Discord.Rest.RestGlobalCommand command in commandList.Result)
            {
                if (command.Name.Contains("temp"))
                {
                    sbTempChannel.AppendLine("");
                    sbTempChannel.AppendLine("**/" + command.Name + "**");
                    sbTempChannel.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sbTempChannel.Append("**/" + command.Name);
                        foreach (var option in command.Options)
                        {
                            sbTempChannel.Append(" <" + option.Name + ">");
                        }
                        sbTempChannel.AppendLine("**");
                    }
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Here is a list of all my commands:")
            .WithColor(0, 225, 225)
            .WithDescription(sbTempChannel.ToString() + sbChannelCommand.ToString())
            .WithFooter(parsedGuild.ToString() + DateTime.Now.ToString(" • dd/MM/yyyy"));
            return embed.Build();

        }

        public static Embed CreateEmbed(SocketInteraction interaction, string body, string header = null)
        {
            var parsedGuild = GetGuildWithInteraction(interaction);

            EmbedBuilder embed = new EmbedBuilder()
            .WithTitle(header)
            .WithColor(0, 225, 225)
            .WithDescription(body)
            .WithFooter(parsedGuild.ToString() + DateTime.Now.ToString(" • dd/MM/yyyy"));

            return embed.Build();
        }

        public static SocketGuild GetGuildWithInteraction(SocketInteraction interaction)
        {
            var parsedArg = (SocketSlashCommand)interaction;
            var parsedGuildUser = (SocketGuildUser)parsedArg.User;
            return (SocketGuild)parsedGuildUser.Guild;
        }
        #endregion
    }
}
