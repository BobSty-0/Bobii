using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.EntityFramework.Entities
{
    public class content
    {
        [Key]
        public int id { get; set; }
        [MaxLength(4)]
        public string msgid { get; set; }
        [MaxLength(400)]
        public string en { get; set; }
        [MaxLength(400)]
        public string de { get; set; }
        [MaxLength(600)]
        public string ru { get; set; }
    }
}
