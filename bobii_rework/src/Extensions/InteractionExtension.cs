using bobii_rework.src.Enums;
using bobii_rework.src.Exceptions;
using Discord;

namespace bobii_rework.Extensions
{
    public static class InteractionExtension
    {
        public static async Task React(this IDiscordInteraction interaction, string content, ResponseType responseType, bool ephemeral = true)
        {
            switch (responseType)
            {
                case ResponseType.Modify:
                    await interaction.DeferAsync(ephemeral);
                    break;
                case ResponseType.Respond:
                    await interaction.RespondAsync(content, ephemeral: ephemeral);
                    break;
                case ResponseType.None:
                    // Hier sollte Bobii einfach nicht rein laufen
                    throw new EnumNotSupportedException();
                default:
                    throw new EnumNotSupportedException();
            }
        }
    }
}
