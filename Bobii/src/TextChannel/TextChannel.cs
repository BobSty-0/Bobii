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

        public static Embed CreateFilterWordEmbed(SocketInteraction interaction, string guildId)
        {
            StringBuilder sb = new StringBuilder();
            var filterWordList = DBStuff.Tables.filterwords.GetCreateFilterWordListFromGuild(guildId);
            string header = null;
            if (filterWordList.Rows.Count == 0)
            {
                header = "No filter words yet!";
                sb.AppendLine("You dont have any filter words yet!\nYou can add some with:\n **/fwadd <FilterWord> <ReplaceWord>**");
            }
            else
            {
                header = "Here a list of all filter words of this Guild:";
            }

            foreach (DataRow row in filterWordList.Rows)
            {
                sb.AppendLine("");
                sb.Append($"Filter word: **{row.Field<string>("filterword")}**");
                sb.AppendLine($" -> Replaced with: **{row.Field<string>("replaceword")}**");
            }
            return TextChannel.CreateEmbed(interaction, sb.ToString(), header);
        }

        //Double Code -> Find solution one day!
        private static string HelpFilterWordInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("**__Filter-Word commands:__**");
            sb.AppendLine("You can filter words out of messages from users, the bot will automatically detect those words, deletes the message and rewrites the message with the bad words replaced");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("fw"))
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
            sb.AppendLine("**__Temporary-Channel commands:__**");
            sb.AppendLine("You can create temporary voice channels which will be created and deleted automatically");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("t"))
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

            var outputBody = HelpTempChannelInfoPart(commandList.Result) + HelpFilterWordInfoPart(commandList.Result);
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
