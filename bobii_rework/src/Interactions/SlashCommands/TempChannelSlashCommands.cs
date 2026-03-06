using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using bobii_rework.src.Enums;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class TempChannelSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [Group(SlashCommandNames.Temp, "Includes all Command to edit temp channels")]
        public class Temp : InteractionModuleBase<InteractionContext>
        {
            [SlashCommand(SlashCommandNames.Name, "Updates the name of the temp channel")]
            private async Task TempName()
            {
                await new TempNameCommand(Context).Execute();
            }

            [SlashCommand(SlashCommandNames.Size, "Updates the size of the temp channel")]
            public async Task TempSize()
            {
                await new TempSizeCommand(Context).Execute();
            }

            [SlashCommand(SlashCommandNames.ClaimOwner, "Updates the owner of the temp channel")]
            public async Task TempClaimOwner()
            {
                await new TempClaimOwnerCommand(Context, InteractionReactionType.Defer).Execute();
            }

            [SlashCommand(SlashCommandNames.GiveOwner, "Updates the owner of the temp channel")]
            public async Task TempGiveOwner()
            {
                await new TempGiveOwnerCommand(Context, InteractionReactionType.Defer).Execute();
            }

            [SlashCommand(SlashCommandNames.Privacy, "Command to manage the privacy of the voice channel")]
            public async Task TempPrivacy()
            {
                await new TempPrivacyCommand(Context, InteractionReactionType.Defer).Execute();
            }
        }
    }
}
