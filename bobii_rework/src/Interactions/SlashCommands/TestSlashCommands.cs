using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class TestSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [SlashCommand("test", "das ist nicht gut")]
        private async Task Test()
        {
            await new TestCommand(Context).Execute();
            //await new HelpCommand(Context).Execute();
            //var channel = await Context.Client.GetChannelAsync(Context.Interaction.ChannelId.Value);
            //await channel.SendGuildJoinedMessage(await LanguageRepository.GetLanguage(Context.Guild.Id));
            //GuildHandler.RemoveGuildFolder(Context.Guild.Id);
            //await new TestCommand(Context).Execute();
            //await Context.Interaction.FollowupAsync("test");
        }
    }
}
