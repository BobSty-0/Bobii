using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class InterfaceInformation
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string CommandName { get; set; }
        public ulong EmoteId { get; set; }
        public string CustomCommandName { get; set; }
        public string CustomCommandColorRGBA { get; set; }
    }
}
