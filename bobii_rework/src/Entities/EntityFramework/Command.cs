using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class Command
    {
        [Key]
        public int id { get; set; }
        [MaxLength(20)]
        public string command { get; set; }
        [MaxLength(100)]
        public string en { get; set; }
        [MaxLength(100)]
        public string de { get; set; }
        [MaxLength(150)]
        public string ru { get; set; }
    }
}
