using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class Emote
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public ulong EmoteId { get; set; }
        public ulong? GuidId { get; set; }
        public bool Animated { get; set; }
    }
}
