namespace Bobii.src.Models
{
    public class TempChannelDelay
    {
        public EntityFramework.Entities.tempchannels TempChannel { get; set; } 
        public TempChannel.DelayDateWrapper DataWrapper { get; set; }
    }
}
