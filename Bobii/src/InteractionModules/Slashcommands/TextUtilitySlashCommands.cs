using Bobii.src.AutocompleteHandler;
using Bobii.src.Bobii;
using Bobii.src.Helper;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.InteractionModules.Slashcommands
{
    class TextUtilitySlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("textutility", "Includes all text-utility commands")]
        public class TextUtility : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("createembed", "This will create an embed with your text")]
            public async Task CreateEmbed()
            {
                var parameter = Context.ContextToParameter();
                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(CreateEmbed)).Result)
                {
                    return;
                }

                var mb = new ModalBuilder()
                    .WithTitle("Create an embed!")
                    .WithCustomId("tucreateembed_modal-nothing")
                    .AddTextInput("Title", "title", TextInputStyle.Short, placeholder: "Insert the title here!", required: false, maxLength: 250)
                    .AddTextInput("Content", "content", TextInputStyle.Paragraph, placeholder: "Insert the content here!", required: false, maxLength: 4000);

                await parameter.Interaction.RespondWithModalAsync(mb.Build());
            }

            [SlashCommand("editembed", "This will edit an embed")]
            public  async Task EditEmbed(
                [Summary("messageid", "Insert the id of the embed message which you want to edit")][Autocomplete(typeof(EditEmbedHandler))] string messageId)
            {
                var parameter = Context.ContextToParameter();

                if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(EditEmbed)).Result)
                {
                    return;
                }

                if (messageId == "1")
                {
                    await parameter.Interaction.RespondAsync(null, new Embed[] { GeneralHelper.CreateEmbed(parameter.Interaction,
                    GeneralHelper.GetContent("C139", parameter.Language).Result,
                    GeneralHelper.GetCaption("C139", parameter.Language).Result).Result });
                    await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(EditEmbed), parameter, message: "No messages detected");
                    return;
                }

                if (Bobii.CheckDatas.CheckMessageID(parameter, messageId, nameof(EditEmbed)).Result ||
                    Bobii.CheckDatas.CheckIfMessageFromCreateEmbed(parameter, ulong.Parse(messageId), nameof(EditEmbed)).Result)
                {
                    return;
                }

                var userMessages = TextUtilityHelper.GetUserMessages(parameter, ulong.Parse(messageId)).Result;

                var mb = new ModalBuilder()
                    .WithTitle($"Edit embed!")
                    .WithCustomId($"tueditembed_modal-{messageId}")
                    .AddTextInput("Title", "title", TextInputStyle.Short, placeholder: "Insert the title here!", required: false, maxLength: 250, value: TextUtilityHelper.GetTitle(userMessages).Result)
                    .AddTextInput("Content", "content", TextInputStyle.Paragraph, placeholder: "Insert the content here!", required: false, maxLength: 4000, value: TextUtilityHelper.GetContent(userMessages).Result);

                await parameter.Interaction.RespondWithModalAsync(mb.Build());
            }
        }
    }
}
