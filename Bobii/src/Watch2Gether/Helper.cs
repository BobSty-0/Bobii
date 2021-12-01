using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Watch2Gether
{
    class Helper
    {
        //Double Code -> Find solution one day!
        public static async Task<string> HelpW2GInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            await Task.CompletedTask;
            return Bobii.Helper.CreateInfoPart(commandList, "You can craete a YouTube watch 2 gether session in any given voice chat, " +
                "simply click the invite link after creating!", "w2g").Result;
        }
    }
}
