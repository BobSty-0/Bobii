using bobii_rework.Extensions;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using Discord;
using Discord.WebSocket;

namespace bobii_rework.Handler.EventHandler
{
    public class JoinedGuildEventHandler : EventHandlerBase
    {
        #region Declarations
        private SocketGuild _guild;
        #endregion

        #region Tasks
        public async Task ExecuteJoinedGuildActionAsync(SocketGuild guild)
        {
            _guild = guild;
            await Execute();
        }
        #endregion

        #region Overrides
        public override async Task ExecuteEvent()
        {
            var bot = _guild.GetUser(Configuration.GetConfigValue<ulong>(Configuration.ApplicationID));
            var socketTextChannel = _guild.TextChannels
                .OrderBy(c => c.Position)
                .FirstOrDefault(c => c.GetChannelType() != ChannelType.Voice && bot.GetPermissions(c).SendMessages);

            if (socketTextChannel == null)
            {
                this.WriteLineToConsole("Keine Channels gefunden um den JoinedGuild Text zu senden");
                return;
            }

            var language = await LanguageRepository.GetLanguage(_guild.Id);

            await socketTextChannel.SendMessageWithEmbedAsync(
                await GeneralHelper.GetJoinedGuildMessageComponent(language),
                "",
                await GeneralHelper.GetJoinedGuildText(language),
                MessageFlags.None);
        }
        #endregion
    }
}
