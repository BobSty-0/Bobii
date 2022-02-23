using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bobii.src.Entities
{
    public class TempChannelDelay
    {
        public EntityFramework.Entities.tempchannels TempChannel { get; set; } 
        public Thread Thread { get; set; }
    }
}
