using bobii_rework.Extensions;
using bobii_rework.Repositories;
using Discord;

namespace bobii_rework.Helper
{
    public static class ButtonHelper
    {
        public static async Task<ButtonBuilder> GetInterfaceButton(string commandName, ulong emoteId)
        {
            Emote emote;
            try
            {
                emote = Emote.Parse($"<:interface_{commandName}:{emoteId}>");
            }
            catch (Exception ex)
            {
                ex.WriteLineToConsole("Emote nicht gefunden");
                var defaultInterfaceInformation = await InterfaceInformationsRepository.GetInterfaceDefaultInformation(commandName);
                emote = Emote.Parse($"<:interface_{commandName}:{defaultInterfaceInformation.EmoteId}");
            }

            return new ButtonBuilder()
                .WithCustomId($"interface-{commandName}-button")
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(emote);
        }
    }
}
