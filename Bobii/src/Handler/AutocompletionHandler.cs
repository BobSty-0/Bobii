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
                case "tcadd":
                    await TempChannel.AutoComplete.AddAutoComplete(interaction);
                    break;
                case "tcupdate":
                    await TempChannel.AutoComplete.UpdateRemoveAutoComplete(interaction);
                    break;
                case "tcremove":
                    await TempChannel.AutoComplete.UpdateRemoveAutoComplete(interaction);
                    break;
                case "tueditembed":
                    await TextUtility.Autocomplete.ChatMessagesAutoComplete(interaction);
                    break;
                case "tempowner":
                    await TempChannel.AutoComplete.TempOwnerAutoComplete(interaction);
                    break;
                case "tempkick":
                    await TempChannel.AutoComplete.TempKickAutoComplete(interaction);
                    break;
            }
        }
    }
}
