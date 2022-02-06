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
                case "tueditembed":
                    await TextUtility.Autocomplete.ChatMessagesAutoComplete(interaction);
                    break;
                case "tempowner":
                    await TempChannel.AutoComplete.TempOwnerAutoComplete(interaction);
                    break;
                case "tempkick":
                    await TempChannel.AutoComplete.TempKickAutoComplete(interaction);
                    break;
                case "flcreate":
                    await FilterLink.AutoComplete.CreateAutoComplete(interaction);
                    break;
                case "fldelete":
                    if (interaction.Data.Current.Name == "name")
                    {
                        await FilterLink.AutoComplete.DeleteNameAutoComplete(interaction);
                    }
                    if (interaction.Data.Current.Name == "link")
                    {
                        await FilterLink.AutoComplete.DeleteLinkAutoComplete(interaction);
                    }
                    break;
                case "logset":
                    await FilterLink.AutoComplete.LinkFilterLogSetAutoComplete(interaction);
                    break;
                case "logupdate":
                    await FilterLink.AutoComplete.LinkFilterLogUpdateAutoComplete(interaction);
                    break;
            }
        }
    }
}
