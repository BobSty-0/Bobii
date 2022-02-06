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
        public static async Task<string> HelpSteaEmojiInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId)
        {
            var sb = new StringBuilder();
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            sb.AppendLine(Bobii.Helper.GetContent("C086", language).Result);

            foreach (Discord.Rest.RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("steal"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(Bobii.Helper.GetCommandDescription(command.Name, language).Result);
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
