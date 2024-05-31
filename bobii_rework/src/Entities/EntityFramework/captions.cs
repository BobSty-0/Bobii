using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class caption
    {
        [Key]
        public int id { get; set; }
        [MaxLength(100)]
        public string msgid { get; set; }
        [MaxLength(50)]
        public string en { get; set; }
        [MaxLength(50)]
        public string de { get; set; }
        [MaxLength(150)]
        public string ru { get; set; }
    }
}
