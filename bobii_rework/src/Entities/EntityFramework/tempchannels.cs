using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class tempchannels
    {
        [Key]
        public long id { get; set; }
        public ulong guildid { get; set; }
        public ulong channelid { get; set; }
        public ulong? createchannelid { get; set; }
        public ulong? channelownerid { get; set; }
        public int count { get; set; }
        public DateTime? deletedate { get; set; }
        public long unixtimestamp { get; set; }
        public bool autoscale { get; set; }
        public ulong? autoscalercategoryid { get; set; }
    }
}
