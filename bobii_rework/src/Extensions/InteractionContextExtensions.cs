using bobii_rework.Entities.Interactions;
using bobii_rework.GlobalConstants.Discord;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.Repositories;
using bobii_rework.src.Enums;
using bobii_rework.src.Exceptions;
using Discord;

namespace bobii_rework.Extensions;

public static class InteractionContextExtensions
{
    public static async Task ReactWithLoadingMessage(this BobiiInteractionContext context, InteractionReactionType interactionReactionType)
    {
        switch (interactionReactionType)
        {
            case InteractionReactionType.Defer:
                await context.Interaction.DeferAsync(true);
                break;
            case InteractionReactionType.Respond:
                var emote = await EmoteRepository.GetEmote(EmoteNames.loading);
                var applicationName = Configuration.GetConfigValue<string>(Configuration.ApplicationName);
                var isThinkingTranslation = await context.GetCaptionAsync(Captions.IsThinking);

                await context.Interaction.RespondAsync($"{emote.ToDiscordEmoteString()} {applicationName} {isThinkingTranslation}", ephemeral: true);
                break;
            case InteractionReactionType.None:
                // Hier sollte Bobii einfach nicht rein laufen
                throw new EnumNotSupportedException();
            default:
                throw new EnumNotSupportedException();
        }
    }
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
            i.Content = null;
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
            i.Content = null;
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