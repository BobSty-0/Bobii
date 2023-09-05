using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Helper
{
    class StealEmojiHelper
    {
        //Double Code -> Find solution one day!
        public static async Task<string> HelpSteaEmojiInfoPart(IReadOnlyCollection<RestGlobalCommand> commandList, ulong guildId)
        {
            var sb = new StringBuilder();
            var language = Bobii.EntityFramework.BobiiHelper.GetLanguage(guildId).Result;
            sb.AppendLine(GeneralHelper.GetContent("C086", language).Result);

            foreach (RestGlobalCommand command in commandList)
            {
                if (command.Name.StartsWith("steal"))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**/" + command.Name + "**");
                    sb.AppendLine(GeneralHelper.GetCommandDescription(command.Name, language).Result);
                    foreach (var cmd in command.Options)
                    {
                        sb.AppendLine("");
                        sb.AppendLine($"</{command.Name} {cmd.Name}:{command.Id}>");
                        sb.AppendLine(GeneralHelper.GetCommandDescription(cmd.Name, language).Result);
                    }
                }
            }
            await Task.CompletedTask;
            return sb.ToString();
        }
    }
}
