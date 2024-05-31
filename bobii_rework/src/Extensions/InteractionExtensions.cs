using bobii_rework.Entities.BobiiSlashCommands;
using bobii_rework.Helper;
using Discord;

namespace bobii_rework.Extensions
{
    public static class InteractionExtensions
    {
        public static async Task RespondWithEmbed(
            this BobiiCommandContext context, 
            string spcHeader, 
            string spcBody, 
            object[]? bodyParameter = null, 
            bool ephemeral = true)
        {
            var body = await context.LanguageRepository!.GetContent(spcBody, context.Language);
            var header = await context.LanguageRepository!.GetCaption(spcHeader, context.Language);

            if (bodyParameter != null)
            {
                body = string.Format(body, bodyParameter);
            }
            
            var embed = EmbedHelper.GetEmbed(body, header);

            await context.Interaction!.RespondAsync(
                null, 
                new[] { embed }, 
                ephemeral: ephemeral);

            context.WriteLineToConsole($"Kommando '/{context.Interaction.Data.Name}' ausgeführt - {header}");
        }

        public static async Task RespondWithSelectionMenu(this BobiiCommandContext context, 
            string customId, 
            List<SelectMenuOptionBuilder> options, 
            string spcPlaceHolder, 
            int maxValue = 1, 
            int minValue = 1,
            bool ephemeral = true)
        {
            var placeHolder = await context.LanguageRepository!.GetContent(spcPlaceHolder, context.Language);
            var selectMenuBuilder = SelectionMenuHelper.GetSelectionMenuBuilder(customId, options, placeHolder, maxValue, minValue);
            var selectMenuComponent = new ComponentBuilder().WithSelectMenu(selectMenuBuilder).Build();

            await context.Interaction!.RespondAsync(components: selectMenuComponent, ephemeral: ephemeral);
            context.WriteLineToConsole($"Kommando '/{context.Interaction.Data.Name}' ausgeführt");
        }
    }
}
