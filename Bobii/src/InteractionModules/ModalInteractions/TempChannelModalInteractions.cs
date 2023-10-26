using Bobii.src.Bobii;
using Bobii.src.EntityFramework.Entities;
using Bobii.src.Helper;
using Bobii.src.Modals;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Bobii.src.Models;
using Bobii.src.Bobii.EntityFramework;
using Bobii.src.TempChannel;

namespace Bobii.src.InteractionModules.ModalInteractions
{
    class TempChannelModalInteractions : InteractionModuleBase<SocketInteractionContext>
    {
        [ModalInteraction("createtempchannel_update_name_modal*,*")]
        public async Task ModalUpdateCreateTempChannelNameResponse(string id, string language, ChangeCreateTempChannelNameModal modal)
        {
            await TempChannel.EntityFramework.CreateTempChannelsHelper.ChangeTempChannelName(modal.NewName, ulong.Parse(id));
            await Context.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
               string.Format(GeneralHelper.GetContent("C171", language).Result, modal.NewName), GeneralHelper.GetCaption("C174", language).Result).Result});
            // TODO Write to console somehow
        }

        [ModalInteraction("tempchannel_update_autodelete_modal*,*")]
        public async Task ModalUpdateCreateTempChannelAutoDeleteResponse(string id, string language, ChangeCreateTempChannelNameModal modal)
        {
            var parameter = Context.ContextToParameter(false);
            var autodelete = modal.NewName;

            if (CheckDatas.CheckAutodelteSize(parameter, autodelete, nameof(ModalUpdateCreateTempChannelAutoDeleteResponse)).Result)
            {
                return;
            }

            var tempChannel = TempChannelsHelper.GetTempChannel(id.ToUlong()).Result;

            if (TempChannelUserConfig.TempChannelUserConfigExists(Context.User.Id, tempChannel.createchannelid.Value).Result)
            {
                var config = TempChannelUserConfig.GetTempChannelConfig(Context.User.Id, tempChannel.createchannelid.Value).Result;
                _ = TempChannelUserConfig.ChangeConfig(Context.Guild.Id, Context.User.Id, tempChannel.createchannelid.Value, config.tempchannelname, config.channelsize.Value, int.Parse(autodelete), config.usernamemode);
            }
            else
            {
                _ = TempChannelUserConfig.AddConfig(Context.Guild.Id, Context.User.Id, tempChannel.createchannelid.Value, "", 0, int.Parse(autodelete), false);
            }

            await Context.Interaction.RespondAsync(ephemeral: true, embeds: new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                      string.Format(GeneralHelper.GetContent("C314", language).Result, modal.NewName), GeneralHelper.GetCaption("C236", language).Result).Result });

            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(ModalUpdateCreateTempChannelAutoDeleteResponse), parameter,
                message: $"autodelete auf {autodelete} gesetzt");
        }

        [ModalInteraction("tempchannel_update_owner_modal")]
        public async Task ModalUpdateTempChannelOwnerResponse(ChangeTempChannelUserModal modal)
        {
            var parameter = Context.Interaction.InteractionToParameter(Context.Client);
            var user = parameter.Guild.Users.FirstOrDefault(user => user.Username == modal.User);

            if (String.IsNullOrEmpty(modal.User) || user == null)
            {
                await Context.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
               string.Format(GeneralHelper.GetContent("C206", parameter.Language).Result, modal.User),
               GeneralHelper.GetCaption("C205", parameter.Language).Result).Result}, ephemeral: true);
                return;
            }

            await TempChannelHelper.TempOwner(parameter, user.Id.ToString());
        }

        [ModalInteraction("tempchannel_kick_user_modal")]
        public async Task ModalTempChannelKickUserResponse(ChangeTempChannelUserModal modal)
        {
            var parameter = Context.Interaction.InteractionToParameter(Context.Client);
            var user = parameter.Guild.Users.FirstOrDefault(user => user.Username == modal.User);

            if (String.IsNullOrEmpty(modal.User) || user == null)
            {
                await Context.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
               string.Format(GeneralHelper.GetContent("C206", parameter.Language).Result, modal.User),
               GeneralHelper.GetCaption("C205", parameter.Language).Result).Result}, ephemeral: true);
                return;
            }

            await TempChannelHelper.TempKick(parameter, new List<string>() { user.Id.ToString() });
        }

        [ModalInteraction("tempchannel_update_size_modal")]
        public async Task ModalUpdateTempChannelSizeResponse(ChangeTempChannelSizeModal modal)
        {
            var parameter = Context.Interaction.InteractionToParameter(Context.Client);
            int newsize = 0;
            if (String.IsNullOrEmpty(modal.NewSize) || !int.TryParse(modal.NewSize, out newsize) || int.Parse(modal.NewSize) < 0)
            {
                await Context.Interaction.RespondAsync(null, new Discord.Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
               string.Format(GeneralHelper.GetContent("C204", parameter.Language).Result, modal.NewSize),
               GeneralHelper.GetCaption("C202", parameter.Language).Result).Result}, ephemeral: true);
                return;
            }

            await TempChannelHelper.TempSize(parameter, newsize);
        }

        [ModalInteraction("tempchannel_update_name_modal*,*")]
        public async Task ModalUpdateTempChannelNameResponse(string id, string language, ChangeCreateTempChannelNameModal modal)
        {
            try
            {
                var parameter = Context.ContextToParameter(false);
                var rateLimitHandler = new RateLimitHandler(parameter);

                var options = new RequestOptions()
                {
                    RatelimitCallback = rateLimitHandler.MyRatelimitCallback
                };

                _ = Task.Run(async () => 
                {
                    await ((SocketVoiceChannel)Context.Client.GetChannel(id.ToUlong())).ModifyAsync(channel => channel.Name = modal.NewName, options: options);

                    await Context.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                    string.Format(GeneralHelper.GetContent("C118", language).Result, modal.NewName),
                    GeneralHelper.GetCaption("C118", language).Result).Result }, ephemeral: true);
                });


                // TODO Write to console somehow
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(Context.Interaction,
                    GeneralHelper.GetContent("C119", language).Result,
                    GeneralHelper.GetCaption("C038", language).Result).Result }, ephemeral: true);
                return;
                // TODO Write to console somehow
            }
        }
    }
}
