using bobii_rework.Entities.BobiiSlashCommands;
using bobii_rework.Extensions;
using bobii_rework.Repositories;
using bobii_rework.Sprachcodes;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.SlashCommands.BobiiSlashCommands
{
    public abstract class BobiiSlashCommandBase
    {
        #region Properties
        public BobiiCommandContext Context { get; }
        #endregion

        #region Constructor
        protected BobiiSlashCommandBase(InteractionContext context)
        {
            Context = GetBobiiCommandContext(context);
        }
        #endregion

        #region Tasks
        public async Task Execute()
        {
            if (await CheckData())
            {
                return;
            }

            await ExecuteCommand();
        }
        #endregion

        #region Abstract Tasks
        public abstract Task<bool> CheckData();
        public abstract Task ExecuteCommand();
        #endregion

        #region Methods
        public async Task<bool> NotEnoughPermissions()
        {
            var guildUser = (SocketGuildUser)Context.User!;

            if (guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild)
            {
                return false;
            }

            await Context.RespondWithEmbed(
                Captions.Error,
                Contents.MissingPermissions,
                new object[] { Context.Interaction!.Data.Name });
            return true;
        }
        #endregion

        #region Private Functions
        private BobiiCommandContext GetBobiiCommandContext(InteractionContext context)
        {
            var languageRepository = new LanguageRepository();

            return new BobiiCommandContext
            {
                Client = context.Client,
                Guild = context.Guild,
                User = context.User,
                Interaction = (ISlashCommandInteraction)context.Interaction,
                LanguageRepository = languageRepository,
                Language = languageRepository.GetLanguage(context.Guild.Id).Result,
            };
        }
        #endregion
    }
}
