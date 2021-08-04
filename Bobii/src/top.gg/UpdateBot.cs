using DiscordBotsList.Api.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.top.gg
{
    class UpdateBot
    {
        public static async Task Update(IDblSelfBot bot, int serverCount)
        {
            await bot.UpdateStatsAsync(serverCount);
        }
    }
}
