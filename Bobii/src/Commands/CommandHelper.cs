using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bobii.src.Commands
{
    class CommandHelper
    {
        #region Methods
        public static void DeletCommandMessage(SocketMessage message)
        {
            message.DeleteAsync();
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands     Message: \"{message.Author}\" was successfully deleted");

        }
        #endregion

        #region Functions
        public static string GetAvatarUrl(SocketUser user, ushort size = 1024)
        {
            return user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();
        }
        #endregion
    }
}
