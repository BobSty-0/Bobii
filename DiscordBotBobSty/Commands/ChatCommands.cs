using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotBobSty.Commands
{
    public class ChatCommands : BaseCommandModule
    {
        public Random Rng { private get; set; }

        [Command("shutup")]
        [Description("'shutup <name> \nCalls the given Name to shut up")]
        public async Task ShutUp(CommandContext ctx, string name)
        {
            await ctx.RespondAsync($"Shut up {name}");
        }

        [Command("random")]
        [Description("'random <number> \nReturns a random number in given range")]
        public async Task RandomNumber(CommandContext ctx, int max)
        {
            await ctx.RespondAsync($"Your random number is: {Rng.Next(max)}");
        }

        //[Command("pagination")]
        //[Description("'pagination \nJust a Test")]
        //public async Task PaginationCommand(CommandContext ctx)
        //{
        //    var reallyLongString = "Phil Collins(*30.Januar 1951 in Chis­wick, London, Eng­land) ist ein briti­scher Schlag­zeuger, Sänger, Song­writer, Pro­duzent, Schau­spieler und Buch­autor. Er wurde sowohl als Mit­glied der Rock­band Genesis als auch als Solo­künst­ler bekannt und gehört mit über 150 Millio­nen ver­kauf­ten Ton­trägern(plus 150 Millio­nen mit Genesis) zu den welt­weit erfolg­reichs­ten Musi­kern der Branche.Collins’ Songs reichen von harten, schlag­zeug­beton­ten Nummern wie In the Air Tonight über ein­gängige Pop - Arrange­ments wie You Can’t Hurry Love bis hin zu balla­desken Titeln mit politi­schen Aus­sagen wie Another Day in Paradise.Trotz dieser Viel­seitig­keit haben Collins’ Schlag­zeug­spiel und seine Gesangs­stimme einen hohen indi­viduel­len Wieder­erkennungs­wert. Im Jahr 2012 wurde er in die Modern Drummer’s Hall of Fame aufgenommen. – Zum Artikel …";

        //    var interactivity = ctx.Client.GetInteractivity();
        //    var pages = interactivity.GeneratePagesInEmbed(reallyLongString);

        //    await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
        //}
    }
}
