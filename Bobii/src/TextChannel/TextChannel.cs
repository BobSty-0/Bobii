﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TextChannel
{
    class TextChannel
    {
        #region Declaration

        #endregion

        #region Methods

        #endregion

        #region Functions

        public static Embed CreateNoWordInfoEmbed(SocketInteraction interaction, string guildId)
        {
            StringBuilder sb = new StringBuilder();
            var createTempChannelList = DBStuff.Tables.badwords.GetCreateBadWordsListFromGuild(guildId);
            string header = null;
            if (createTempChannelList.Rows.Count == 0)
            {
                header = "No BadWords yet!";
                sb.AppendLine("You dont have any BadWords yet!\nYou can add some with:\n **/badwordadd <BadWord> <ReplaceWord>**");
            }
            else
            {
                header = "Here a list of all BadWords:";
            }

            foreach (DataRow row in createTempChannelList.Rows)
            {
                sb.AppendLine("");
                sb.AppendLine($"BadWord: **{row.Field<string>("badword")}**");
                sb.AppendLine($"ReplaceWord: **{row.Field<string>("replaceword")}**");
            }
            return TextChannel.CreateEmbed(interaction, sb.ToString(), header);
        }

        //Double Code -> Find solution one day!
        private static string HelpBadWordInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("**__BadWord commands:__**");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("badword"))
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

            var outputBody = HelpTempChannelInfoPart(commandList.Result) + HelpBadWordInfoPart(commandList.Result);
            //712373862179930144 -> BobSty Guild
            if (!Commands.SlashCommands.CheckIfItsBobSty(interaction, guildid, user, parsedArg, "", false) && user.Guild.Id == 712373862179930144)
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

        public static Embed CreateEmbedWithoutTitle(string body, string guildname)
        {
            EmbedBuilder embed = new EmbedBuilder()
            .WithColor(0, 225, 225)
            .WithDescription($"**{body}**")
            .WithFooter(guildname + DateTime.Now.ToString(" • dd/MM/yyyy"));

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
