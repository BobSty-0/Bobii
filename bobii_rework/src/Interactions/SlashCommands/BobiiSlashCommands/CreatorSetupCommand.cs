using bobii_rework.src.Components;
using Discord.Interactions;
using System.Reflection.Metadata;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class CreatorSetupCommand : BobiiInteractionBase
    {
        #region Constructor
        public CreatorSetupCommand(InteractionContext context) : base(context)
        {
        }
        #endregion

        #region Methods
        public override async Task ExecuteCommand()
        {
            await Context.Interaction!.DeferAsync();

            var category = await Context.Guild!.CreateCategoryAsync(await Context.GetCaptionAsync(Captions.DefaultCategoryName));

            var textChannel = await Context.Guild!.CreateTextChannelAsync(
                await Context.GetCaptionAsync(Captions.DefaultInterfaceTextChatName),
                p => p.CategoryId = category.Id);

            var voiceChannel = await Context.Guild!.CreateVoiceChannelAsync(
                await Context.GetCaptionAsync(Captions.DefaultCreatorChannelName),
                p => p.CategoryId = category.Id);

            await CreatorChannelRepository.AddCreatorChannel(
                Context.Guild!.Id,
                await Context.GetCaptionAsync(Captions.DefaultTempChannelName),
                voiceChannel.Id,
                0,
                0,
                0);

            await textChannel.SendInterface(Context, voiceChannel.Id);

            await Context.FollowUpWithEmbedAsync(Captions.Success, Contents.SetupSuccessfull, new object[] {voiceChannel.Id});
        }

        public override Task<bool> CheckData()
        {
            return NotEnoughPermissions();
        }
        #endregion
    }
}
