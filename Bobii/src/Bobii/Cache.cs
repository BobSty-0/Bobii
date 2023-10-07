using Bobii.src.TempChannel.EntityFramework;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    public class Cache
    {
        public List<src.EntityFramework.Entities.caption> Captions { get; set; }
        public List<src.EntityFramework.Entities.content> Contents { get; set; }
        public List<src.EntityFramework.Entities.commands> Commands { get; set; }
        public List<src.EntityFramework.Entities.tempchannels> TempChannels { get; set; }
        public List<src.EntityFramework.Entities.createtempchannels> CreateTempChannels { get; set; }
        public List<src.EntityFramework.Entities.tempchanneluserconfig> TempChannelUserConfigs { get; set; }
        public List<src.EntityFramework.Entities.tempcommands> TempCommands { get; set; }

        public void ResetTempChannelsCache()
        {
            Task.Run(() =>TempChannels = TempChannelsHelper.GetTempChannelList().Result);
        }

        public void ResetCreateTempChannelsCache()
        {
            Task.Run(() => CreateTempChannels = CreateTempChannelsHelper.GetCreateTempChannelList().Result);
        }

        public void ResetTempChanneluserConfigsCache()
        {
            Task.Run(() => TempChannelUserConfigs = TempChannelUserConfig.GetTempChannelConfigsList().Result);
        }

        public void ResetTempCommandsCache()
        {
            Task.Run(() => TempCommands = TempCommandsHelper.GetDisabledCommandsList().Result);
        }
    }
}
