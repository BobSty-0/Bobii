using Discord;

namespace Bobii.src.Models
{
    class ApplicationCommandOptionTypes
    {
        public int Integer { get; set; }

        public bool Boolean { get; set; }

        public IGuildChannel IGuildChannel { get; set; }

        public IMentionable IMentionable { get; set; }

        public double Double { get; set; }

        public IRole IRole { get; set; }

        public string String { get; set; }

        public object SubCommand { get; set; }

        public object SubCommandGroup { get; set; }

        public IUser IUser { get; set; }
    }
}
