using Bobii.src.Bobii;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.Slashcommands
{
    class BobiiSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        #region Declarations
        public InteractionService _interactionService;
        public SocketGuild _mainGuild;
        #endregion

        public BobiiSlashCommands(InteractionService interactionService, SocketGuild mainGuild)
        {
            _interactionService = interactionService;
            _mainGuild = mainGuild;
        }

        [SlashCommand("comregister", "Registers all commands of Bobii")]
        public  async Task ComRegister()
        {
            // TODO nur bestimmte commands global registrieren
            // und das auch bei EventHandler einbauen um ComRegister zu registrieren
            await _interactionService.AddCommandsToGuildAsync(_mainGuild, false, _interactionService.SlashCommands.Where(x => x.Name == "comregister").ToArray());
            await _interactionService.AddCommandsGloballyAsync(false, _interactionService.SlashCommands.Where(x => x.Name == "info" || 
            x.Name == "creatcommandlist" ||
            x.Name == "add" ||
            x.Name == "name" ||
            x.Name == "size").ToArray());
        }
    }
}
