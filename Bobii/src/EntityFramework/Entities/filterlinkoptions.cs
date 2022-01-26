using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
{
    class filterlinkoptions
    {
        [Key]
        public int id { get; set; }
        [MaxLength(50)]
        public string bezeichnung { get; set; }
        [MaxLength(100)]
        public string linkbody { get; set; }
        [MaxLength(18)]
        public ulong? guildid { get; set; }
    }
}
