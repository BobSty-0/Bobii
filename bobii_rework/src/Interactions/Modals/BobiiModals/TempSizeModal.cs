using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord.Interactions;

namespace bobii_rework.Interactions.Modals.BobiiModals
{
    internal class TempSizeModal(InteractionContext context, string size) : BobiiInteractionBase(context, ResponseType.Respond)
    {

        #region  Tasks        
        public override async Task ExecuteCommand()
        {
            await Context.User!.VoiceChannel.ModifyAsync(channel => channel.UserLimit = int.Parse(size));
            await Context.RespondOrModifyOriginalResponse(Captions.Success, Contents.TempSizeChanged, new object[] { size });
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
                   await IsInteger(size);
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
