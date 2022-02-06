using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
{
    public class commands
    {
        [Key]
        public int id { get; set; }
        [MaxLength(20)]
        public string command { get; set; }
        [MaxLength(100)]
        public string en { get; set; }
        [MaxLength(100)]
        public string de { get; set; }
    }
}
