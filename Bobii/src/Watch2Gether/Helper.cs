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
            var sb = new StringBuilder();
            sb.AppendLine("You can craete a YouTube watch 2 gethere session in any given voice chat, simply klick the invite link after creating!");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("w2g"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(command.Description);
                    if (command.Options != null)
                    {
                        sb.Append("**/" + command.Name);
                        foreach (var option in command.Options)
                        {
                            sb.Append(" <" + option.Name + ">");
                        }
                        sb.AppendLine("**");
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }
    }
}
