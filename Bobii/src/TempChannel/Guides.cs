using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class Guides
    {
        public static async Task<string> StepByStepTcadd()
        {
            await Task.CompletedTask;
            return "**Step 1:**\n" +
                                "Think of a good name for the temp channel\n" +
                                "This will be the name which the created temp-channel will have.\n" +
                                "A feature which I've added is that `User` in the temp-channel name will be replaced with the username of the User who joined the create-temp-channel.\n" +
                                "Example: \n" +
                                "If I use `User's Channel` as temp-channel name the name of the temp-channel would be:\n" +
                                "`BobSty's Channel` (_because my username is BobSty_)\n\n" +

                                "**Step 2:**\n" +
                                "Use the command `/tcadd` und press the `Tab` key on your keyboard.\n" +
                                "Now there should be a list appearing from all voice channels from your server.\n" +
                                "Simply click on the one which you want to add as create-temp-channel and press the `Tab` key on your keyboard again.\n" +
                                "Here you should enter the name which you thought of in step 1.\n" +
                                "Press `Enter` on your keyboard and the create-temp-channel should be added.\n\n" +
                                
                                "**Step 3:**\n" +
                                "Test the create-temp-channel simply by joining the the voice channel which you used in step 2\n\n" +
                                "If you have any issues with this command/guid feel free to send a direct message to <@776028262740393985>";
        }
    }
}
