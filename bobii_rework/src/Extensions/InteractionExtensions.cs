using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.Entities.BobiiSlashCommands;
using Discord;

namespace bobii_rework.Extensions;

public static class InteractionExtensions
{
    public static async Task<string> GetCaptionAsync(this BobiiInteractionContext bobiiContext, string spcCaption)
    {
        return await LanguageRepository.GetCaption(spcCaption, bobiiContext.Language);
    }

    public static async Task<string> GetContentAsync(this BobiiInteractionContext bobiiContext, string spcContent)
    {
        return await LanguageRepository.GetContent(spcContent, bobiiContext.Language);
    }

    public static async Task FollowUpWithEmbedAsync(this BobiiInteractionContext bobiiContext,
        string spcHeader,
        string spcBody,
        object[]? bodyParameter = null,
        bool ephemeral = true)
    {
        if (!bobiiContext.Interaction!.HasResponded)
        {
            await bobiiContext.Interaction!.DeferAsync();
        }

        var body = await bobiiContext.GetContentAsync(spcBody);
        var header = await bobiiContext.GetCaptionAsync(spcHeader);

        if (bodyParameter != null)
        {
            body = string.Format(body, bodyParameter);
        }

        var embed = EmbedHelper.GetEmbed(body, header);

        await bobiiContext.Interaction.FollowupAsync(embeds: new [] { embed }, ephemeral: ephemeral);
    }

    public static async Task ModifyOriginalResponseWithEmbedAsync(this BobiiInteractionContext bobiiContext, string body)
    {
        var embed = EmbedHelper.GetEmbed(body);

        if (!bobiiContext.Interaction!.HasResponded)
        {
            await bobiiContext.Interaction!.DeferAsync();
        }

        await bobiiContext.Interaction!.ModifyOriginalResponseAsync(msg => msg.Embeds = new[] { embed });
    }

    public static async Task RespondWithEmbedAsync(
        this BobiiInteractionContext bobiiContext,
        string spcHeader,
        string spcBody,
        object[]? bodyParameter = null,
        bool ephemeral = true)
    {
        var body = await bobiiContext.GetContentAsync(spcBody);
        var header = await bobiiContext.GetCaptionAsync(spcHeader);

        if (bodyParameter != null)
        {
            body = string.Format(body, bodyParameter);
        }

        var embed = EmbedHelper.GetEmbed(body, header);

        await bobiiContext.Interaction!.RespondAsync(
            embeds: new[] { embed },
            ephemeral: ephemeral);
    }

    public static async Task RespondWithSelectionMenuAsync(this BobiiInteractionContext bobiiContext,
        string customId,
        List<SelectMenuOptionBuilder> options,
        string spcPlaceHolder,
        int maxValue = 1,
        int minValue = 1,
        bool ephemeral = true)
    {
        var placeHolder = await bobiiContext.GetContentAsync(spcPlaceHolder);
        var selectMenuBuilder = SelectionMenuHelper.GetSelectionMenuBuilder(customId, options, placeHolder, maxValue, minValue);
        var selectMenuComponent = new ComponentBuilder().WithSelectMenu(selectMenuBuilder).Build();

        await bobiiContext.Interaction!.RespondAsync(components: selectMenuComponent, ephemeral: ephemeral);
    }
}