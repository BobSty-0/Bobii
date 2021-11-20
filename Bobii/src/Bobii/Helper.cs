using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    class Helper
    {
        #region Tasks
        public static async Task<string> CreateServerCount(DiscordSocketClient client)
        {
            var sb = new StringBuilder();
            foreach (var guild in client.Guilds)
            {
                sb.AppendLine($"**Name:** {guild.Name} | **Membercount:** {guild.MemberCount}");
            }
            sb.AppendLine();
            sb.AppendLine($"**Servercount:** {client.Guilds.Count}");
            await Task.CompletedTask;
            return sb.ToString();
        }

        public static async Task RefreshBobiiStats()
        {
            await Handler.HandlingService.RefreshServerCountChannels();
        }

        public static async Task<string> HelpSupportPart()
        {
            await Task.CompletedTask;
            return "If you have any questions, you can simply send <@776028262740393985> a direct message. I will try to answer you as soon as possible!\nAlso if you have found a bug or an error I would appreciate if you report it via direct message to <@776028262740393985> so I can fix it asap.";
        }

        //Double Code -> Find solution one day!
        private static async Task<string> HelpCommandInfoPart(IReadOnlyCollection<RestGuildCommand> commandList)
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
            await Task.CompletedTask;
            return sb.ToString();
        }

        public static async Task<Embed> CreateEmbed(SocketInteraction interaction, string body, string header = null)
        {
            var parsedGuild = GetGuildWithInteraction(interaction);

            EmbedBuilder embed = new EmbedBuilder()
            .WithTitle(header)
            .WithColor(74, 171, 189)
            .WithDescription(body)
            .WithFooter(parsedGuild.Result.Name + DateTime.Now.ToString(" • dd/MM/yyyy"));

            await Task.CompletedTask;
            return embed.Build();
        }

        public static async Task<Embed> CreateEmbed(SocketGuild guild, string body, string header = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(header)
                .WithColor(74, 171, 189)
                .WithDescription(body)
                .WithFooter(guild.Name + DateTime.Now.ToString(" • dd/MM/yyyy"));
            await Task.CompletedTask;
            return embed.Build();
        }

        public static async Task<SocketGuild> GetGuildWithInteraction(SocketInteraction interaction)
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
            await Task.CompletedTask;
            return null;
        }
        #endregion
    }
}
