using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
{
    class tempchannels
    {
        [Key]
        public int id { get; set; }
        [MaxLength(18)]
        public ulong guildid { get; set; }
        [MaxLength(18)]
        public ulong channelid { get; set; }
        [MaxLength(18)]
        public ulong? createchannelid { get; set; }
        [MaxLength(18)]
        public ulong? channelownerid { get; set; }
        public int count { get; set; }
        public ulong? textchannelid { get; set; }
    }
}
