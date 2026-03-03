using bobii_rework.Entities.EntityFramework;
using bobii_rework.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.Repositories
{
    public class EmoteRepository
    {
        public static async Task<Emote?> GetEmote(string name)
        {
            await using var context = new BobiiContext();
            return await context.Emotes.SingleOrDefaultAsync(e => e.Name == name);
        }
    }
}
