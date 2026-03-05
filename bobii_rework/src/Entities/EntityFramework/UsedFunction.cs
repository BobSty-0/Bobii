using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bobii_rework.Entities.EntityFramework
{
    public class UsedFunction
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string Function { get; set; }
        public ulong UserId { get; set; }
        public ulong AffectedUserId { get; set; }
        [Column(TypeName = "timestamp without time zone")] public DateTime DoneAt { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
        public bool IsUser { get; set; }
    }
}
