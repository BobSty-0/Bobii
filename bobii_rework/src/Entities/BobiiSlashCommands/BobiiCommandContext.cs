using bobii_rework.Enums;
using bobii_rework.Repositories;
using Discord;

namespace bobii_rework.Entities.BobiiSlashCommands
{
    public class BobiiCommandContext
    {
        public IDiscordClient? Client { get; set; }
        public IGuild? Guild { get; set; }
        public IUser? User { get; set; }
        public Language Language { get; set; }
        public ISlashCommandInteraction? Interaction { get; set; }
        public LanguageRepository? LanguageRepository { get; set; }
    }
}
