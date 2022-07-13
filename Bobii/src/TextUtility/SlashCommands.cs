using Bobii.src.Models;
using Discord;
using System.Threading.Tasks;

namespace Bobii.src.TextUtility
{
    class SlashCommands
    {
        #region Embeds
        public static async Task CreateEmbed(SlashCommandParameter parameter)
        {
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

        public static async Task EditEmbed(SlashCommandParameter parameter)
        {
            if (Bobii.CheckDatas.CheckUserPermission(parameter, nameof(EditEmbed)).Result)
            {
                return;
            }

            var messageId = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "messageid").Result.String;

            if (messageId == Bobii.Helper.GetContent("C138", parameter.Language).Result.ToLower())
            {
                await parameter.Interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(parameter.Interaction,
                    Bobii.Helper.GetContent("C139", parameter.Language).Result,
                    Bobii.Helper.GetCaption("C139", parameter.Language).Result).Result });
                await Handler.HandlingService.BobiiHelper.WriteToConsol(src.Bobii.Actions.SlashComms, true, nameof(EditEmbed), parameter, message: "No messages detected");
                return;
            }

            messageId = messageId.Split(' ')[0];

            if (Bobii.CheckDatas.CheckMessageID(parameter, messageId, nameof(EditEmbed)).Result ||
                Bobii.CheckDatas.CheckIfMessageFromCreateEmbed(parameter, ulong.Parse(messageId), nameof(EditEmbed)).Result)
            {
                return;
            }

            var userMessages = Helper.GetUserMessages(parameter, ulong.Parse(messageId)).Result;

            var mb = new ModalBuilder()
                .WithTitle($"Edit embed!")
                .WithCustomId($"tueditembed_modal-{messageId}")
                .AddTextInput("Title", "title", TextInputStyle.Short, placeholder: "Insert the title here!", required: false, maxLength: 250, value: Helper.GetTitle(userMessages).Result)
                .AddTextInput("Content", "content", TextInputStyle.Paragraph, placeholder: "Insert the content here!", required: false, maxLength: 4000, value: Helper.GetContent(userMessages).Result);

            await parameter.Interaction.RespondWithModalAsync(mb.Build());


            var embedTitle = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "title").Result.String;
            var embedContent = Handler.SlashCommandHandlingService.GetOptionWithName(parameter, "content").Result.String;
        }
        #endregion
    }
}
