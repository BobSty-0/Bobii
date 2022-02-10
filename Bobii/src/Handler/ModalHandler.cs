using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class ModalHandler
    {
        public static async Task HandleModal(SocketModal modal)
        {
            switch (modal.Data.CustomId)
            {
                case "tcadd_modal":

                    break;
            }
        }
    }
}
