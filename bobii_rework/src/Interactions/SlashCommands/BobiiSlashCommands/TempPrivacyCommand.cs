using bobii_rework.Extensions;
using bobii_rework.Repositories;
using bobii_rework.src.Helper;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TempPrivacyCommand(InteractionContext context) : BobiiInteractionBase(context)
    {
        public override async Task ExecuteCommand()
        {
            var comboBox = await MessageComponentHelper.GetTempPrivacyMessageComponent(Context.Language);
            await Context.ModifyOriginalResponse(comboBox);
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
                   await CommandIsDisabled(tempChannel);
        }
    }
}
