using bobii_rework.EntityFramework;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using File = bobii_rework.Entities.EntityFramework.File;

namespace bobii_rework.src.Repositories
{
    public class FileRepository
    {
        public static async Task<File> CreateFile(byte[] data, byte[] hash, MagickFormat format = MagickFormat.WebP)
        {
            await using var context = new BobiiContext();

            var file = new File()
            {
                Data = data,
                Format = format,
                Hash = hash
            };

            await context.Files.AddAsync(file);
            await context.SaveChangesAsync();

            return file;
        }

        public static async Task<File?> GetFile(byte[] hash)
        {
            await using var context = new BobiiContext();
            return await context.Files.SingleOrDefaultAsync(f => f.Hash == hash);
        }
    }
}
