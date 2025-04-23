using bobii_rework.Extensions;
using bobii_rework.Interactions.SlashCommands.BobiiSlashCommands;
using bobii_rework.Repositories;
using Discord.Interactions;

namespace bobii_rework.Interactions.SlashCommands
{
    public class TestSlashCommands : InteractionModuleBase<InteractionContext>
    {
        [SlashCommand("test", "das ist nicht gut")]
        private async Task Test()
        {
            var language = await LanguageRepository.GetLanguage(Context.Guild.Id!);
            await Context.Interaction.RespondWithLoadingMessage();
            await Task.Delay(5000);
            await Context.Interaction.ModifyOriginalResponseAsync(e =>
            {
                e.Attachments = null;
                e.Content = "Nice man";
            });
            //await new HelpCommand(Context).Execute();
            //var channel = await Context.Client.GetChannelAsync(Context.Interaction.ChannelId.Value);
            //await channel.SendGuildJoinedMessage(await LanguageRepository.GetLanguage(Context.Guild.Id));
            //GuildHandler.RemoveGuildFolder(Context.Guild.Id);
            //await new TestCommand(Context).Execute();
            //await Context.Interaction.FollowupAsync("test");
        }
    }
}
