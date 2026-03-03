using bobii_rework.Entities.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;
using File = bobii_rework.Entities.EntityFramework.File;

namespace bobii_rework.Extensions
{
    public static class ChannelExtensions
    {
        public static async Task SendFileWithEmbedAsync(
            this IChannel channel,
            BobiiInteractionContext bobiiContext,
            File file,
            MessageComponent components,
            string spcHeader,
            string spcBody,
            MessageFlags flags = MessageFlags.SuppressNotification)
        {
            var textChannel = (ITextChannel)channel;
            var body = await bobiiContext.GetContentAsync(spcBody);
            var header = await bobiiContext.GetCaptionAsync(spcHeader);
            var fileName = $"File{GeneralHelper.GetFileExtension(file.Format)}";
            var embed = EmbedHelper.GetEmbed(body, header, $"attachment://{fileName}");
            using var ms = new MemoryStream(file.Data);
            ms.Position = 0;

            await textChannel.SendFileAsync(
                ms,
                fileName,
                embeds: [embed],
                components: components,
                flags: flags);
        }

        public static async Task SendMessageWithEmbedAsync(
            this IChannel channel,
            MessageComponent components,
            string header,
            string body,
            MessageFlags flags = MessageFlags.SuppressNotification)
        {
            var textChannel = (ITextChannel)channel;
            var embed = EmbedHelper.GetEmbed(body, header);

            await textChannel.SendMessageAsync(
                embeds: [embed],
                components: components,
                flags: flags);
        }

        public static async Task SendInterface(this IChannel channel, BobiiInteractionContext context, ulong creatorChannelId)
        {
            var interfaceImage = await InterfaceHelper.CreateInterfaceImg(context, creatorChannelId);
            var interfaceButtons = await InterfaceHelper.GetInterfaceButtons(context, creatorChannelId);
            await channel.SendFileWithEmbedAsync(context, interfaceImage, interfaceButtons.Build(), Captions.InterfaceCaption, Contents.InterfaceText);
        }
    }
}
