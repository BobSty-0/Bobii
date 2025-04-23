using Discord.Interactions;
using System.Reflection.Metadata;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class CreatorSetupCommand : BobiiInteractionBase
    {
        #region Constructor
        public CreatorSetupCommand(InteractionContext context) : base(context)
        {
        }
        #endregion

        #region Tasks
        public override async Task ExecuteCommand()
        {
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
            var dashboardUrl = Configuration.GetConfigValue<string>(Configuration.DashboardUrl)!;

            await Context.ModifyOriginalResponse(
                Captions.Success,
                Contents.SetupSuccessfull,
                new object[] { voiceChannel.Id, dashboardUrl },
                messageComponent: await GetButtonMessageComponent(dashboardUrl));
        }

        public override async Task<bool> CheckData()
        {
            return  await NotEnoughPermissions();
        }
        #endregion

        #region Private Functions
        private async Task<MessageComponent> GetButtonMessageComponent(string url)
        {
            return new ComponentBuilder()
                .WithButton(await ButtonHelper.GetDashboardButton(Context.Language))
                .Build();
        }
        #endregion
    }
}
