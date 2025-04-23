using System.Diagnostics;
using Discord;
using Discord.Interactions;
using System.Reflection.Metadata;
using bobii_rework.Handler.UtilityHandler;
using bobii_rework.Entities.Interactions.Modals;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord.WebSocket;

namespace bobii_rework.Interactions.Modals.BobiiModals
{
    internal class TempNameModal : BobiiInteractionBase
    {
        #region Declarations
        private Entities.Interactions.Modals.TempNameModalEntity _modalEntity;
        #endregion

        #region Consturctor
        public TempNameModal(InteractionContext context, Entities.Interactions.Modals.TempNameModalEntity modalEntity) : base(context)
        {
            _modalEntity = modalEntity;
        }
        #endregion

        #region Tasks
        public override async Task ExecuteCommand()
        {
            var voiceChannel = (SocketVoiceChannel)Context.User!.VoiceChannel;
            await SetName(voiceChannel);
            await SetStatus(voiceChannel);

            if (Context.HasResponded)
            {
                return;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Success,
                Contents.StatusAndNameChanged,
                new object[] { _modalEntity.Name, _modalEntity.Status });
        }

        public async Task SetName(SocketVoiceChannel voiceChannel)
        {
            if (voiceChannel.Name == _modalEntity.Name)
            {
                return;
            }

            var options = new RequestOptions { RatelimitCallback = new RateLimitHandler(Context).MyRatelimitCallback };
            await voiceChannel.ModifyAsync(v => v.Name = _modalEntity.Name, options: options);
        }

        public async Task SetStatus(SocketVoiceChannel voiceChannel)
        {
            if (Context.HasResponded)
            {
                return;
            }

            if (voiceChannel.Status != _modalEntity.Status)
            {
                await voiceChannel.SetStatusAsync(_modalEntity.Status);
            }
        }
        #endregion
    }
}
