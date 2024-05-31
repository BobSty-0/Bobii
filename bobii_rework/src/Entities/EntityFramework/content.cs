using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class content
    {
        [Key]
        public int id { get; set; }
        [MaxLength(100)]
        public string msgid { get; set; }
        [MaxLength(400)]
        public string en { get; set; }
        [MaxLength(400)]
        public string de { get; set; }
        [MaxLength(600)]
        public string ru { get; set; }
    }
}
