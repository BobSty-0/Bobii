using Bobii.src.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class SubCommandHandler
    {
        public static async Task HandleSubCommands(SlashCommandParameter parameter)
        {
            switch (parameter.SlashCommand.Data.Options.First().Name)
            {
                case "commands":
                    await Bobii.SlashCommands.BobiiHelp(parameter);
                    break;
                case "guides":
                    await Bobii.SlashCommands.BobiiGuides(parameter);
                    break;
                case "support":
                    await Bobii.SlashCommands.BobiiSupport(parameter);
                    break;
            }
        }
    }
}
