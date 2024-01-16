using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Modals
{
    class ChangeCreateTempChannelNameModal : IModal
    {
        public string Title => string.Empty;
        [ModalTextInput("new_name")]
        public string NewName { get; set; }
        [ModalTextInput("new_status")]
        public string NewStatus { get; set; }
    }

    class ChangeTempChannelSizeModal : IModal
    {
        public string Title => string.Empty;
        [ModalTextInput("new_size")]
        public string NewSize { get; set; }
    }

    class ChangeTempChannelUserModal : IModal
    {
        public string Title => string.Empty;
        [ModalTextInput("user")]
        public string User { get; set; }
    }
}
