using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
{
    public class createtempchannels
    {
        [Key]
        public int id { get; set; }
        [MaxLength(18)]
        public ulong guildid { get; set; }
        [MaxLength(50)]
        public string tempchannelname { get; set; }
        [MaxLength(18)]
        public ulong createchannelid { get; set; }
        public int? channelsize { get; set; }
        public bool? textchannel { get; set; }
        public int? delay { get; set; }
    }
}
