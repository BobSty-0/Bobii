using Bobii.src.DBStuff.Tables;
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
        public static Embed CreateFilterLinkUserWhitelistInfoEmbed(SocketInteraction interaction, ulong guildid)
        {
            StringBuilder sb = new StringBuilder();
            var userOnWhitelist = DBStuff.Tables.filterlinkuserguild.GetUsers(guildid);
            string header = null;

            if (userOnWhitelist.Rows.Count == 0)
            {
                header = "No whitelisted users yet!";
                sb.AppendLine("You dont have any users on the whitelist yet!\nYou can add users to the whitelist with:\n **/fluadd <link>**");
            }
            else
            {
                header = "Here is a list of all the users on the whitelist of this guild";
            }

            foreach(DataRow row in userOnWhitelist.Rows)
            {
                sb.AppendLine("");
                sb.AppendFormat($"<@{row.Field<string>("userid")}>");
            }

            var filterLinkActiveText = "";
            if (!filterlink.IsFilterLinkActive(guildid.ToString()))
            {
                filterLinkActiveText = "\n\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
            }
            return TextChannel.CreateEmbed(interaction, sb.ToString() + filterLinkActiveText, header);
        }

        public static Embed CreateFilterLinkLinkWhitelistInfoEmbed(SocketInteraction interaction, ulong guildId)
        {
            StringBuilder sb = new StringBuilder();
            var filterLinksOnWhitelist = DBStuff.Tables.filterlinksguild.GetLinks(guildId);
            string header = null;

            if (filterLinksOnWhitelist.Rows.Count == 0)
            {
                header = "No whitelisted links yet!";
                sb.AppendLine("You dont have any links on the whitelist yet!\nYou can add links to the whitelist with:\n **/flladd <link>**");
            }
            else
            {
                header = "Here is a list of all the links on the whitelist of this guild";
            }

            foreach (DataRow row in filterLinksOnWhitelist.Rows)
            {
                sb.AppendLine("");
                sb.AppendLine($"{row.Field<string>("bezeichnung")}");
            }

            var filterLinkActiveText = "";
            if (!filterlink.IsFilterLinkActive(guildId.ToString()))
            {
                filterLinkActiveText = "\nFilter link is currently inactive, to activate filter link use:\n`/flset <active>`";
            }
            return TextChannel.CreateEmbed(interaction, sb.ToString() + filterLinkActiveText, header);
        }

        public static Embed CreateFilterWordEmbed(SocketInteraction interaction, string guildId)
        {
            StringBuilder sb = new StringBuilder();
            var filterWordList = DBStuff.Tables.filterwords.GetFilterWordListFromGuild(guildId);
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

        private static string FLLHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("_Manage the links of the whitelist:_");
            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("fll"))
                {
                    sb.AppendLine("");
                    sb.AppendLine($"**/{command.Name}**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append($"**/{command.Name}");
                        foreach (var option in command.Options)
                        {
                            sb.Append($" <{option.Name}>");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            return sb.ToString();
        }

        private static string FLUHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("_Manage the users of the whitelist:_");
            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("flu"))
                {
                    sb.AppendLine("");
                    sb.AppendLine($"**/{command.Name}**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append($"**/{command.Name}");
                        foreach (var option in command.Options)
                        {
                            sb.Append($" <{option.Name}>");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            return sb.ToString();
        }

        private static string FLLogHelpTeil(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("_Manage the log channel of filter link:_");
            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("log"))
                {
                    sb.AppendLine("");
                    sb.AppendLine($"**/{command.Name}**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append($"**/{command.Name}");
                        foreach (var option in command.Options)
                        {
                            sb.Append($" <{option.Name}>");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            return sb.ToString();
        }

        //Double Code -> Find solution one day!
        public static string HelpFilterLinkInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Filter link will block every kind of links as soon as you activated it. You can then start whitelisting links which wont be blocked and users which will not be affected by filter link. I currently only have a couple of choices for links to whitelist so if you want to whitelist an link which I forgot to provide as choice feel free to message me on Discord:\n`BobSty#0815`");

            //Filterlink in generall
            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.Contains("flinfo") || command.Name.Contains("flset"))
                {
                    sb.AppendLine("");
                    sb.AppendLine($"**/{command.Name}**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append($"**/{command.Name}");
                        foreach (var option in command.Options)
                        {
                            sb.Append($" <{option.Name}>");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            return sb.ToString() + FLLHelpTeil(commandList) + FLUHelpTeil(commandList) + FLLogHelpTeil(commandList);
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

        public static Embed CreateEmbed(SocketGuild guild, string body, string header = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(0, 225, 225)
                .WithDescription(body)
                .WithFooter(guild.Name + DateTime.Now.ToString(" • dd/MM/yyyy"));
            return embed.Build();
        }

        public static SocketGuild GetGuildWithInteraction(SocketInteraction interaction)
        {
            if (interaction.Type == InteractionType.MessageComponent)
            {
                var parsedArg = (SocketMessageComponent)interaction;
                var parsedGuildUser = (SocketGuildUser)parsedArg.User;
                return (SocketGuild)parsedGuildUser.Guild;
            }
            if (interaction.Type == InteractionType.ApplicationCommand)
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
