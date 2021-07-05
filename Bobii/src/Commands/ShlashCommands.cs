using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    class ShlashCommands
    {
        public static async Task SlashCommandHandler(SocketInteraction interaction)
        {
            switch (interaction.Data.ToString())
            {
                case "example":
                    await interaction.RespondAsync("test");
                    break;
            }
        }
    }
}
