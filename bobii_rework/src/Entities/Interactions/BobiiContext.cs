using bobii_rework.Repositories;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bobii_rework.Enums;

namespace bobii_rework.src.Entities.BobiiSlashCommands
{
    public class BobiiInteractionContext
    {
        public IDiscordClient? Client { get; set; }
        public IGuild? Guild { get; set; }
        public IUser? User { get; set; }
        public Language Language { get; set; }
        public IDiscordInteraction? Interaction { get; set; }
    }
}
