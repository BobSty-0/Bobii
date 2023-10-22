using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
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
