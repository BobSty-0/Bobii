using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TextUtility
{
    class Helper
    {
        //Double Code -> Find solution one day!
        public static async Task<string> HelpTextUtilityInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You can create embeds with <@776028262740393985> for clean looking announcement for example!\nNote: you can use **<br>** for linebreaks");

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("tu"))
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
