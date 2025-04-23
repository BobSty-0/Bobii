using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord.Interactions;

namespace bobii_rework.Interactions.Modals.BobiiModals
{
    internal class TempSizeModal : BobiiInteractionBase
    {
        #region Declarations
        private string _size;
        #endregion

        #region Constructors
        public TempSizeModal(InteractionContext context, string size) : base(context)
        {
            _size = size;
        }
        #endregion

        #region  Tasks        
        public override async Task ExecuteCommand()
        {
            await Context.User!.VoiceChannel.ModifyAsync(channel => channel.UserLimit = int.Parse(_size));
            await Context.RespondOrModifyOriginalResponse(Captions.Success, Contents.TempSizeChanged, new object[] {_size});
        }

        public override async Task<bool> CheckData()
        {
            if (await UserNotInVoice())
            {
                return true;
            }

            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);

            return await UserNotInTempChannel(tempChannel) ||
                   await NotTheChannelOwnerOrMod(tempChannel) ||
                   await CommandIsDisabled(tempChannel) ||
                   await IsInteger(_size);
        }

        public async Task<bool> IsInteger(string value)
        {
            if (int.TryParse(value, out _))
            {
                return false;
            }

            await Context.RespondOrModifyOriginalResponse(
                Captions.Error,
                Contents.ValueIstKeineZahl);
            return true;
        }
    #endregion

}
}
