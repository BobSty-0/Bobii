using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class tempcommands
    {
        [Key]
        public long id { get; set; }
        public string commandname { get; set; }
        public bool enabled { get; set; }
        public ulong guildguid { get; set; }
        public ulong createchannelid { get; set; }
    }
}
