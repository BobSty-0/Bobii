using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    static class Extensions
    {
        public static string Link2LinkOptions(this string str)
        {
            str = str.Replace("https://", "");
            str = str.Replace("http://", "");
            str = str.Split('/')[0];
            str = $"{str}/";
            return str;
        }

        public static ulong ToUlong(this string str)
        {
            return ulong.Parse(str);
        }
    }
}
