using System.ComponentModel.DataAnnotations;

namespace bobii_rework.Entities.EntityFramework
{
    public class InterfaceInformation
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string CommandName { get; set; }
        public int EmoteId { get; set; }
        public string? CustomCommandName { get; set; }
        public string? CustomCommandBackgroundColorHex { get; set; }
        public string? CustomCommandForeColorHex { get; set; }
        public int Sort { get; set; }
        // Gibt an ob die InnterfaceInformation im Interface mit abgedruckt wird oder ob es sich um eine Info für z.B. eine ComboBox handelt
        public bool IsCommand { get; set; }
    }
}
