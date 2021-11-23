using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.StealEmoji
{
    class Helper
    {
        //Double Code -> Find solution one day!
        public static async Task<string> HelpSteaEmojiInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You can steal emoji's from other servers (only if you have nitro).\nSimply use the emoji which you want to have in your server and give it a name.\n**This is not quite legal, so I take no responsibility!**");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("steal"))
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
