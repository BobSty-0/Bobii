using Bobii.src.AutocompleteHandler;
using Bobii.src.Bobii;
using Bobii.src.Handler;
using Bobii.src.Helper;
using Bobii.src.TempChannel.EntityFramework;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Communication.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bobii.src.InteractionModules.Slashcommands
{
    class SetUpdateModeSlashCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("setupdatemode", "Meow only command")]
        public async Task SetUpdateMode()
        {
            var parameter = Context.ContextToParameter();

            if (parameter.GuildUser.Id != ulong.Parse("410312323409117185"))
            {
                await parameter.Interaction.RespondAsync(embeds: new Embed[] { GeneralHelper.CreateEmbed(parameter.Guild, "You dont have permission to use this command!", "", true).Result });
                return;
            }

            HandlingService.DontReact = true;

            await Program.SetBotUpdateStatusAsync(parameter.Client);

            await parameter.Interaction.RespondAsync(embeds: new Embed[] { GeneralHelper.CreateEmbed(parameter.Guild, "Updatemodus erfolgreich gesetzt", "", false).Result });
            await Handler.HandlingService.BobiiHelper.WriteToConsol(Actions.SlashComms, false, nameof(SetUpdateMode), parameter, message: $"Update mode gesetzt: Aktiv");
            return;
        }
    }
}

