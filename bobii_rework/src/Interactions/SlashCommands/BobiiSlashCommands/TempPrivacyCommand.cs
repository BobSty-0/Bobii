using bobii_rework.Extensions;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using bobii_rework.src.Helper;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands.BobiiSlashCommands
{
    public class TempPrivacyCommand(InteractionContext context, InteractionReactionType interactionReactionType) : BobiiInteractionBase(context, interactionReactionType)
    {
        public override async Task ExecuteCommand()
        {
            var comboBox = await MessageComponentHelper.GetTempPrivacyMessageComponent(Context.Guild!.Id, Context.Language);
            await Context.ModifyOriginalResponse(comboBox);
        }

        public override async Task<bool> CheckData()
        {
            if (await UserNotInVoice())
            {
                return true;
            }

            var tempChannel = await TempChannelRepository.GetTempChannel(Context.User!.VoiceChannel.Id);

            // TODO hier eine schöner Lösunng finden, im CheckData sollte eigentlich nichts gesetzt werden
            await TransferOwnerIfOwnerNotInVoice(tempChannel);

            return await UserNotInTempChannel(tempChannel) ||
                   await NotTheChannelOwnerOrMod(tempChannel) ||
                   await CommandIsDisabled(tempChannel);
        }
    }
}
