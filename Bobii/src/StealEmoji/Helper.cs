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

            foreach (RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("steal"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(Bobii.Helper.GetCommandDescription(command.Name, language).Result);
                    foreach (var cmd in command.Options)
                    {
                        sb.AppendLine("");
                        sb.AppendLine("**/" + command.Name + " " + cmd.Name + "**");
                        sb.AppendLine(Bobii.Helper.GetCommandDescription(cmd.Name, language).Result);
                        if (cmd.Options.Count > 0)
                        {
                            sb.Append("**/" + command.Name + " " + cmd.Name);
                            foreach (var option in cmd.Options)
                            {
                                sb.Append(" <" + option.Name + ">");
                            }
                            sb.AppendLine("**");
                        }
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }
    }
}
