using ImageMagick;
using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class File
    {
        [Key] public int Id { get; set; }
        public byte[] Data { get; set; }
        public MagickFormat Format { get; set; }
        public byte[] Hash { get; set; }
    }
}
