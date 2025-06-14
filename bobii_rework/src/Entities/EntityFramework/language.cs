using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class Language
    {
        [Key]
        public int id { get; set; }
        [MaxLength(18)]
        public ulong guildid { get; set; }
        [MaxLength(2)]
        public string langugeshort { get; set; }
    }
}
