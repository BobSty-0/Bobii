using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class TempChannelSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [Group("temp", "Includes all commands to edit temp channels")]
        public class Temp : InteractionModuleBase<ShardedInteractionContext>
        {
            [SlashCommand("name", "Updates the name of the temp channel")]
            private async Task TempName()
            {

            }

            [SlashCommand("size", "Updates the size of the temp channel")]
            public async Task TempSize()
            {

            }

            [SlashCommand("claimowner", "Updates the owner of the temp channel")]
            public async Task TempClaimOwner()
            {
            }

            [SlashCommand("giveowner", "Updates the owner of the temp channel")]
            public async Task TempGiveOwner()
            {
            }
        }
    }
}
