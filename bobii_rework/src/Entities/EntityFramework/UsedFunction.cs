using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class UsedFunction
    {
        [Key]
        public int id { get; set; }
        [MaxLength(30)]
        public string function { get; set; }
        public ulong userid { get; set; }
        public ulong affecteduserid { get; set; }
        public DateTime doneat { get; set; }
        public ulong channelid { get; set; }
        public ulong guildid { get; set; }
        public bool isuser { get; set; }
    }
}
