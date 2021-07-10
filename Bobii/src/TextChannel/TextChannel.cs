using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        //Double Code -> Find solution one day!
        private static string HelpTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("**__TempChannel commands:__**");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("temp"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append("**/" + command.Name);
                        foreach (var option in command.Options)
                        {
                            sb.Append(" <" + option.Name + ">");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            return sb.ToString();
        }

        //Double Code -> Find solution one day!
        private static string HelpCommandInfoPart(IReadOnlyCollection<RestGuildCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("**__Manage-Command commands:__**");

            foreach (Discord.Rest.RestGuildCommand command in commandList)
            {
                if (command.Name.Contains("com"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append("**/" + command.Name);
                        foreach (var option in command.Options)
                        {
                            sb.Append(" <" + option.Name + ">");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            return sb.ToString();
        }

        public static Embed CreateHelpInfoEmbed(string guildid, SocketInteraction interaction, DiscordSocketClient client)
        {
            var parsedArg = (SocketSlashCommand)interaction;
            var user = (SocketGuildUser)parsedArg.User;
            var parsedGuild = GetGuildWithInteraction(interaction);
            var commandList = client.Rest.GetGlobalApplicationCommands();
            var bobGuildCommandList = client.Rest.GetGuildApplicationCommands(parsedGuild.Id);

            var outputBody = HelpTempChannelInfoPart(commandList.Result);
            if (!Commands.SlashCommands.CheckIfItsBobSty(interaction, guildid, user, parsedArg, "", false))
            {
                outputBody = outputBody + HelpCommandInfoPart(bobGuildCommandList.Result);
            }

            EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Here is a list of all my commands:")
            .WithColor(0, 225, 225)
            .WithDescription(outputBody)
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
