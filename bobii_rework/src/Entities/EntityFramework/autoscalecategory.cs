using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class autoscalecategory
    {
        [Key]
        public int id { get; set; }
        public ulong guildid { get; set; }
        [MaxLength(50)]
        public string channelname { get; set; }
        public ulong categoryid { get; set; }
        public int emptychannelnumber { get; set; }
        public int? channelsize { get; set; }
        public int? autodelete { get; set; }
    }
}
