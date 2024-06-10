using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.Entities.BobiiSlashCommands;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.src.Components
{
    public abstract class BobiiInteractionBase
    {
        #region Properties
        public BobiiInteractionContext Context { get; }
        #endregion

        #region Constructor
        protected BobiiInteractionBase(InteractionContext context)
        {
            Context = GetBobiiInteractionContext(context);
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

        #region Overridables
        public virtual Task<bool> CheckData()
        {
            return Task.FromResult(false);
        }

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

            var slashCommandInteraction = (ISlashCommandInteraction)Context.Interaction!;
            await Context.RespondWithEmbedAsync(
                Captions.Error,
                Contents.MissingPermissions,
                new object[] { slashCommandInteraction!.Data.Name });
            return true;
        }
        #endregion

        #region Private Functions
        private BobiiInteractionContext GetBobiiInteractionContext(InteractionContext context)
        {
            return new BobiiInteractionContext
            {
                Client = context.Client,
                Guild = context.Guild,
                User = context.User,
                Interaction = context.Interaction,
                Language = LanguageRepository.GetLanguage(context.Guild.Id).Result,
            };
        }
        #endregion
    }
}
