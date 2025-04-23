using bobii_rework.GlobalConstants.Interactions;
using Discord.Interactions;

namespace bobii_rework.Entities.Interactions.Modals
{
    public class TempNameModalEntity : IModal
    {
        public string Title => string.Empty;
        [ModalTextInput(ModalInputCustomIds.Name)]
        public string Name { get; set; }
        [ModalTextInput(ModalInputCustomIds.Status)]
        public string Status { get; set; }
    }
}
