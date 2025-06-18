using bobii_rework.Entities.EntityFramework;
using bobii_rework.Entities.Interactions;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace bobii_rework.Interactions
{
    public abstract class BobiiInteractionBase
    {
        #region Declarations
        private bool _respondWithLoadingMessage;
        #endregion

        #region Properties
        public BobiiInteractionContext Context { get; }
        #endregion

        #region Constructor
        protected BobiiInteractionBase(InteractionContext context, bool respondWithLoadingMessage = true)
        {
            Context = GetBobiiInteractionContext(context);
            _respondWithLoadingMessage = respondWithLoadingMessage;
        }
        #endregion

        #region Tasks
        public async Task Execute()
        {
            try
            {
                if (_respondWithLoadingMessage)
                {
                    await Context.Interaction!.RespondWithLoadingMessage();
                }

                if (await CheckData())
                {
                    return;
                }

                await ExecuteCommand();
            }
            catch (Exception ex)
            {
                this.WriteLineToConsole($"{ex.Message} | {ex.StackTrace}");
            }

        }
        #endregion

        #region Overridables
        public virtual async Task<bool> CheckData()
        {
            return false;
        }

        public abstract Task ExecuteCommand();
        #endregion

        #region Methods
        public async Task<bool> UserNotInVoice()
        {
            if (Context.User!.VoiceChannel != null)
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.NotInVoice,
                []);
            return true;
        }

        public async Task<bool> GivenUserNotInVoice(IGuildUser user)
        {
            if (user!.VoiceChannel != null)
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.GivenUserNotInVoice,
                [user.Id]);
            return true;
        }

        public async Task<bool> GivenUserNotInSameChannel(IGuildUser givenUser)
        {

            if (Context.User!.VoiceChannel.Id == givenUser.VoiceChannel.Id)
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.GivenUserNotInSameChannel,
                [givenUser.Id]);
            return true;
        }

        public async Task<bool> UserNotInTempChannel(TempChannel? tempChannel)
        {
            if (tempChannel != null)
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.NotInTempChannel,
                []);
            return true;
        }

        public async Task<bool> CommandIsDisabled(TempChannel tempChannel)
        {
            if (InteractionType.ApplicationCommand != Context.Interaction!.Type)
            {
                return false;
            }

            var slashCommandInteraction = (ISlashCommandInteraction)Context.Interaction!;
            var commandDisabled = await TempCommandRepository.CommandDisabled(
                Context.Guild!.Id,
                tempChannel.createchannelid!.Value,
                slashCommandInteraction!.Data.Name);

            if (!commandDisabled)
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.CommandDisabled,
                new object[] { $"/{SlashCommandNames.Temp} {slashCommandInteraction!.Data.Name}" });
            return true;
        }

        public async Task<bool> NotTheChannelOwnerOrMod(TempChannel tempChannel)
        {
            if (!await NotTheChannelOwner(tempChannel, false))
            {
                return false;
            }

            var usedModsFunction = await UsedFunctionsRepository.GetUsedUserFunction(
                SlashCommandNames.Moderator,
                Context.Guild!.Id,
                Context.User!.Id);

            var moderatorCommandDisabled = await TempCommandRepository.CommandDisabled(
                Context.Guild!.Id,
                tempChannel.createchannelid!.Value,
                SlashCommandNames.Moderator);

            if (!moderatorCommandDisabled && usedModsFunction == null)
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.NotTheOwner,
                [tempChannel.channelownerid!]);
            return true;
        }

        public async Task<bool> NotTheChannelOwner(TempChannel tempChannel, bool respond)
        {
            if (tempChannel.channelownerid == Context.User!.Id)
            {
                return false;
            }

            if (respond)
            {
                await Context.RespondOrModifyOriginalResponse(
                    Captions.Error,
                    Contents.NotTheOwner,
                    new object[] { tempChannel.channelownerid! });
            }

            return true;
        }

        public async Task<bool> NotEnoughPermissions()
        {
            var guildUser = (SocketGuildUser)Context.User!;

            if (guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild)
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.MissingPermissions);
            return true;
        }
        #endregion

        #region Private Functions
        private BobiiInteractionContext GetBobiiInteractionContext(InteractionContext context)
        {
            var bobiiContext = new BobiiInteractionContext
            {
                Client = context.Client,
                Interaction = context.Interaction,
            };

            // Bei DM events gibt es keine Guild
            if (context.Guild == null)
            {
                return bobiiContext;
            }

            bobiiContext.Guild = context.Guild;
            bobiiContext.User = (IGuildUser)context.User;
            bobiiContext.Language = LanguageRepository.GetLanguage(context.Guild.Id).Result;

            return bobiiContext;
        }
        #endregion
    }
}
