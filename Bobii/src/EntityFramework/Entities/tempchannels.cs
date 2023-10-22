using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
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
