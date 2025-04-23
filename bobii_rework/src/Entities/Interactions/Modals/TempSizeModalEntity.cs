using bobii_rework.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Entities.Interactions.Modals
{
    public class TempSizeModalEntity : IModal
    {
        public string Title => string.Empty;
        [ModalTextInput(ModalInputCustomIds.Size)]
        public string Size { get; set; }
    }
}
