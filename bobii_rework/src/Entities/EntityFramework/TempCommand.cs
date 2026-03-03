using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class TempCommand
    {
        [Key]
        public long Id { get; set; }
        public string CommandName { get; set; }
        public bool Enabled { get; set; }
        public ulong GuildId { get; set; }
        public ulong CreateChannelId { get; set; }
    }
}
