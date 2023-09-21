using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
{
    public class usedfunctions
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
