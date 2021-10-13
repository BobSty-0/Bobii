using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class AutocompletionHandlingService
    {
        public static async Task HandleAutocompletion(SocketAutocompleteInteraction interaction)
        {
            switch (interaction.Data.CommandName)
            {
                case "flladd":
                    await FilterLink.AutoHelper.AddAutoComplete(interaction);
                    break;
                case "fllremove":
                    await FilterLink.AutoHelper.RemoveAutoComplete(interaction);
                    break;
                case "tcadd":
                    await TempChannel.AutoHelper.AddAutoComplete(interaction);
                    break;
                case "tcupdate":
                    await TempChannel.AutoHelper.UpdateRemoveAutoComplete(interaction);
                    break;
                case "tcremove":
                    await TempChannel.AutoHelper.UpdateRemoveAutoComplete(interaction);
                    break;
            }
        }
    }
}
