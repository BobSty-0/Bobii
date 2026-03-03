using bobii_rework.Entities.Interactions;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using Discord;
using Discord.WebSocket;

namespace bobii_rework.Helper
{
    public static class ModalHelper
    {
        public static async Task<ModalBuilder> GetTempSizeModal(BobiiInteractionContext context)
        {
            var tempChannelEditCaption = await context.GetCaptionAsync(Captions.EditChannel);
            var tempChannelSizeCaption = await context.GetCaptionAsync(Captions.Size);

            return new ModalBuilder()
                .WithTitle(tempChannelEditCaption)
                .WithCustomId(ModalCustomIds.TempChannelSize)
                .AddTextInput(
                    tempChannelSizeCaption,
                    ModalInputCustomIds.Size,
                    required: true,
                    maxLength: 2,
                    value: context.User!.VoiceChannel.UserLimit.GetValueOrDefault().ToString());
        }

        public static async Task<ModalBuilder> GetTempNameModal(BobiiInteractionContext context)
        {
            var tempChannelEditCaption = await context.GetCaptionAsync(Captions.EditChannel);
            var tempChannelNameCaption = await context.GetCaptionAsync(Captions.Name);
            var tempChannelStatusCaptions = await context.GetCaptionAsync(Captions.Status);

            var voiceChannel = (SocketVoiceChannel)context.User!.VoiceChannel;

            return new ModalBuilder()
                .WithTitle(tempChannelEditCaption)
                .WithCustomId(ModalCustomIds.TempChannelName)
                .AddTextInput(tempChannelNameCaption, ModalInputCustomIds.Name, TextInputStyle.Short, required: true, maxLength: 25, value: context.User.VoiceChannel.Name)
                .AddTextInput(tempChannelStatusCaptions, ModalInputCustomIds.Status, TextInputStyle.Short, required: false, maxLength: 25, value: voiceChannel.Status);
        }
    }
}
