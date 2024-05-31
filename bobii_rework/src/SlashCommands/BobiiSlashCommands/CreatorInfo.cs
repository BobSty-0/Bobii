using bobii_rework.Entities.EntityFramework;
using bobii_rework.Extensions;
using bobii_rework.Repositories;
using bobii_rework.SelectionMenus;
using bobii_rework.Sprachcodes;
using Discord;
using Discord.Interactions;

namespace bobii_rework.SlashCommands.BobiiSlashCommands
{
    public class CreatorInfo : BobiiSlashCommandBase
    {
        #region Consturctor
        public CreatorInfo(InteractionContext context) : base(context)
        {
        }
        #endregion

        #region Methods
        public override Task<bool> CheckData()
        {
            return NotEnoughPermissions();
        }

        public override async Task ExecuteCommand()
        {
            try
            {
                var creatorChannelRepository = new CreatorChannelRepository();
                var creatorChannels = await creatorChannelRepository.GetCreatorChannels(Context.Guild!.Id);

                if (!creatorChannels.Any())
                {
                    await Context.RespondWithEmbed(
                        Captions.Error,
                        Contents.NoCreatorChannels);
                    return;
                }

                var selectionMenuOptions = await GetSelectionMenuOptions(creatorChannels);

                await Context.RespondWithSelectionMenu(
                    SelectMenuCustomIds.CreatorInfo, 
                    selectionMenuOptions,
                    Contents.WaehleCreatorChannel);
            }
            catch (Exception ex)
            {
                this.WriteLineToConsole(ex.Message);
            }
        }
        #endregion

        #region Private Functions
        private async Task<List<SelectMenuOptionBuilder>> GetSelectionMenuOptions(List<createtempchannels> creatorChannels)
        {
            var selectionMenuOptions = new List<SelectMenuOptionBuilder>();
            var channels = await Context.Guild!.GetChannelsAsync();

            foreach (var creatorChannel in creatorChannels)
            {
                var channel = channels.SingleOrDefault(c => c.Id == creatorChannel.createchannelid);
                if (channel == default(IGuildChannel))
                {
                    continue;
                }

                var option = new SelectMenuOptionBuilder()
                    .WithValue(creatorChannel.createchannelid.ToString())
                    .WithLabel(channel.Name);

                selectionMenuOptions.Add(option);
            }

            return selectionMenuOptions;
        }
        #endregion
    }
}
