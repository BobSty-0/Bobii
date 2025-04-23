using System.Text;
using bobii_rework.Entities.Interactions;
using bobii_rework.Enums;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;

namespace bobii_rework.Extensions
{
    public static class ChannelExtensions
    {
        public static async Task SendFileWithEmbedAsync(
            this IChannel channel,
            BobiiInteractionContext bobiiContext,
            string imagePath,
            MessageComponent components,
            string spcHeader,
            string spcBody,
            MessageFlags flags = MessageFlags.SuppressNotification)
        {
            var textChannel = (ITextChannel)channel;
            var body = await bobiiContext.GetContentAsync(spcBody);
            var header = await bobiiContext.GetCaptionAsync(spcHeader);
            var embed = EmbedHelper.GetEmbed(body, header, $"attachment://{Path.GetFileName(imagePath)}");

            await textChannel.SendFileAsync(
                imagePath, 
                embeds: new[] { embed }, 
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
                embeds: new[] { embed },
                components: components,
                flags: flags);
        }

        public static async Task SendInterface(this IChannel channel, BobiiInteractionContext context, ulong creatorChannelId)
        {
            var imageFilePath = await InterfaceHelper.CreateInterfaceImg(context, creatorChannelId);
            var interfaceButtons = await InterfaceHelper.GetInterfaceButtons(context, creatorChannelId);
            await channel.SendFileWithEmbedAsync(context, imageFilePath, interfaceButtons.Build(), Captions.InterfaceCaption, Contents.InterfaceText);
        }
    }
}
