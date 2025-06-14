using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class CreateTempChannel
    {
        [Key]
        public int id { get; set; }
        public ulong guildid { get; set; }
        [MaxLength(50)]
        public string tempchannelname { get; set; }
        public ulong createchannelid { get; set; }
        public int? channelsize { get; set; }
        public int? delay { get; set; }
        public int? autodelete { get; set; }
    }
}
