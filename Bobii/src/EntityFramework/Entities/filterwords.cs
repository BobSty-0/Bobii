using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
{
    class filterwords
    {
        [Key]
        public int id { get; set; }
        [MaxLength(18)]
        public ulong guildid { get; set; }
        [MaxLength(20)]
        public string filterword { get; set; }
        [MaxLength(20)]
        public string replaceword { get; set; }
    }
}
