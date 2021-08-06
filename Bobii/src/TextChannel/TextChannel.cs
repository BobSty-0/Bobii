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
        public static string HelpFilterWordInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You can replace unwanted words from users' messages. I will automatically detect the words, delete the user's message and create a new message in which the unwanted words are replaced.\nTo start, simply add a filter word.");

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
        public static string HelpTempChannelInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You can create temporary voice channels which are created and deleted automatically.\nTo get a instructions on how to use certain commands use the command: `/bobiiguides`!");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("tc"))
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

        public static Embed CreateFilterWordEmbed(SocketUser user, string guildName, string body)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor(user)
                .WithColor(0, 225, 225)
                .WithDescription(body)
                .WithFooter(guildName + DateTime.Now.ToString(" • dd/MM/yyyy"));
            return embed.Build();
        }

        public static SocketGuild GetGuildWithInteraction(SocketInteraction interaction)
        {
            if(interaction.Type == InteractionType.MessageComponent)
            {
                var parsedArg = (SocketMessageComponent)interaction;
                var parsedGuildUser = (SocketGuildUser)parsedArg.User;
                return (SocketGuild)parsedGuildUser.Guild;
            }
            if(interaction.Type == InteractionType.ApplicationCommand)
            {
                var parsedArg = (SocketSlashCommand)interaction;
                var parsedGuildUser = (SocketGuildUser)parsedArg.User;
                return (SocketGuild)parsedGuildUser.Guild;
            }
            //Should never happen!
            return null;
        }
        #endregion
    }
}
