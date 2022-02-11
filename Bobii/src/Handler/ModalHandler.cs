using Bobii.src.Entities;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class ModalHandler
    {
        public static async Task HandleModal(SocketModal modal, DiscordSocketClient client)
        {
            var parameter = new SlashCommandParameter();

            parameter.Modal = modal;
            parameter.GuildUser = (SocketGuildUser)modal.User;
            parameter.Guild = parameter.GuildUser.Guild;
            parameter.GuildID = parameter.Guild.Id;
            parameter.Client = client;
            parameter.Interaction = (SocketInteraction)modal;
            parameter.Language = Bobii.EntityFramework.BobiiHelper.GetLanguage(parameter.Guild.Id).Result;

            switch (modal.Data.CustomId.Split('-')[0])
            {
                case "tucreateembed_modal":
                    await TextUtility.Modals.CreateEmbed(parameter);
                    break;
                case "tueditembed_modal":
                    await TextUtility.Modals.EditEmbed(parameter, modal.Data.CustomId.Split('-')[1]);
                    break;
            }
        }
    }
}
