using Discord.WebSocket;
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
                    await FilterLink.AutoComplete.AddAutoComplete(interaction);
                    break;
                case "fllremove":
                    await FilterLink.AutoComplete.RemoveAutoComplete(interaction);
                    break;
                case "tcadd":
                    await TempChannel.AutoComplete.AddAutoComplete(interaction);
                    break;
                case "tcupdate":
                    await TempChannel.AutoComplete.UpdateRemoveAutoComplete(interaction);
                    break;
                case "tcremove":
                    await TempChannel.AutoComplete.UpdateRemoveAutoComplete(interaction);
                    break;
            }
        }
    }
}
