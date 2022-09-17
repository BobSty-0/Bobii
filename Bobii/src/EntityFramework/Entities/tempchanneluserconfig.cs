using System.ComponentModel.DataAnnotations;

namespace Bobii.src.EntityFramework.Entities
{
    class tempchanneluserconfig
    {
        [Key]
        public int id { get; set; }
        public ulong guildid { get; set; }
        public ulong userid { get; set; }
        public ulong createchannelid { get; set; }
        [MaxLength(50)]
        public string tempchannelname { get; set; }
        public int? channelsize { get; set; }
    }
}
