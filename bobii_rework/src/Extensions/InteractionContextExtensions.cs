using bobii_rework.Entities.Interactions;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using Discord;

namespace bobii_rework.Extensions;

public static class InteractionContextExtensions
{
    public static async Task<string> GetCaptionAsync(this BobiiInteractionContext bobiiContext, string spcCaption)
    {
        return await LanguageRepository.GetCaption(spcCaption, bobiiContext.Language);
    }

    public static async Task<string> GetContentAsync(this BobiiInteractionContext bobiiContext, string spcContent)
    {
        return await LanguageRepository.GetContent(spcContent, bobiiContext.Language);
    }

    public static async Task RespondOrModifyOriginalResponse(this BobiiInteractionContext bobiiContext,
        string spcHeader,
        string spcBody,
        object[]? bodyParameter = null,
        MessageComponent? messageComponent = null)
    {

        if (bobiiContext.Interaction!.HasResponded)
        {
            await bobiiContext.ModifyOriginalResponse(
                spcHeader,
                spcBody,
                bodyParameter,
                messageComponent);
            return;
        }

        await bobiiContext.RespondAsync(
            spcHeader,
            spcBody,
            bodyParameter,
            messageComponent);
    }

    public static async Task RespondAsync(
        this BobiiInteractionContext bobiiContext,
        string spcHeader,
        string spcBody,
        object[]? bodyParameter = null,
        MessageComponent? messageComponent = null,
        bool ephemeral = true)
    {
        var body = await bobiiContext.GetContentAsync(spcBody);
        var header = await bobiiContext.GetCaptionAsync(spcHeader);

        if (bodyParameter != null)
        {
            body = string.Format(body, bodyParameter);
        }

        messageComponent = messageComponent ?? new ComponentBuilder().Build();
        body = body.Replace("****", "");

        var embed = EmbedHelper.GetEmbed(body, header);

        await bobiiContext.Interaction!.RespondAsync(
            embeds: new[] { embed },
            components: messageComponent,
            ephemeral: ephemeral);
    }

    public static async Task ModifyOriginalResponse(this BobiiInteractionContext bobiiContext,
        MessageComponent messageComponent,
        bool ephemeral = true)
    {
        await bobiiContext.Interaction.ModifyOriginalResponseAsync(i =>
        {
            i.Components = messageComponent;
            i.Attachments = null;
        });
    }

    public static async Task ModifyOriginalResponse(this BobiiInteractionContext bobiiContext,
        string spcHeader,
        string spcBody,
        object[]? bodyParameter = null,
        MessageComponent? messageComponent = null)
    {
        if (!bobiiContext.Interaction!.HasResponded)
        {
            await bobiiContext.Interaction!.DeferAsync();
        }

        var body = await bobiiContext.GetContentAsync(spcBody);
        var header = await bobiiContext.GetCaptionAsync(spcHeader);

        if (bodyParameter is { Length: > 0 })
        {
            body = string.Format(body, bodyParameter);
        }

        messageComponent = messageComponent ?? new ComponentBuilder().Build();
        body = body.Replace("****", "");

        var embed = EmbedHelper.GetEmbed(body, header);

        await bobiiContext.Interaction.ModifyOriginalResponseAsync(i =>
        {
            i.Embeds = new[] { embed };
            i.Components = messageComponent;
            i.Attachments = null;
        });
    }

    public static async Task ModifyEmbedOfOriginalResponse(this BobiiInteractionContext bobiiContext, string body)
    {
        var embed = EmbedHelper.GetEmbed(body);

        if (!bobiiContext.Interaction!.HasResponded)
        {
            await bobiiContext.Interaction!.DeferAsync();
        }

        await bobiiContext.Interaction!.ModifyOriginalResponseAsync(msg => msg.Embeds = new[] { embed });
    }

    public static async Task ModifyEmbedAndComponentsFromOriginalResponse(this BobiiInteractionContext bobiiContext, string body, MessageComponent components)
    {
        var embed = EmbedHelper.GetEmbed(body);

        if (!bobiiContext.Interaction!.HasResponded)
        {
            await bobiiContext.Interaction!.DeferAsync();
        }

        await bobiiContext.Interaction!.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embeds = new[] { embed };
            msg.Components = components;
        });
    }
}