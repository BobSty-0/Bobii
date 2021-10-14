using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.FilterLink
{
    class Guides
    {
        public static async Task<string> StepByStepFLLAdd()
        {
            await Task.CompletedTask;
            return "**Step 1:**\n" +
                "Use the command `/flladd` and press the `Tab` key on your keyboard.\n\n" +
                "**Step 2:**\n" +
                "Choose the kind of link you want to add to the white list.\n" +
                "Example:\n" +
                "If I add youtube to the whitelist then all kind of youtube links will be ignored by Bobii.\n" +
                "_Bobii will only filter links if filter link is active!_ (/flset <active>)\n\n" +
                "If you have any issues with this command/guid feel free to send a direct message to <@776028262740393985>";
        }
    }
}
