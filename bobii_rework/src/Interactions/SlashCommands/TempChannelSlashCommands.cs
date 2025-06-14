using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
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
            }

            [SlashCommand(SlashCommandNames.GiveOwner, "Updates the owner of the temp channel")]
            public async Task TempGiveOwner()
            {
            }
        }
    }
}
