
using bobii_rework.Repositories;
using bobii_rework.src.Entities.BobiiSlashCommands;
using bobii_rework.src.GlobalConstants.Interactions;
using Discord;
using ImageMagick;
using SkiaSharp;
using System.Text;
using System.Text.RegularExpressions;
using bobii_rework.Extensions;

namespace bobii_rework.Helper
{
    public static class InterfaceHelper
    {
        #region Methods
        public static async Task<string> CreateInterfaceImg(BobiiInteractionContext context, ulong creatorChannelId)
        {
            var filePath = GetInterfaceFileName(context, creatorChannelId);

            if (File.Exists(filePath))
            {
                return filePath;
            }

            var commandNames = await GetTempCommands(context);
            var interfaceImg = GetMagickImage(commandNames.Count());
            var commandInfoImages = await GetCommandInfoImages(context, commandNames);

            AddCommandInfoImagesToInterfaceImage(interfaceImg, commandInfoImages);
            await SaveInterfaceImage(interfaceImg, context, creatorChannelId);

            return GetInterfaceFileName(context, creatorChannelId);
        }

        public static async Task<MagickImage> GetCommandImage(BobiiInteractionContext context, string commandName)
        {
            var emoteId = await InterfaceInformationsRepository.GetInterfaceEmoteIdWithFallback(context.Guild!.Id, commandName);
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://cdn.discordapp.com/emojis/{emoteId}.png");
            response.EnsureSuccessStatusCode();

            await using var imageStream = await response.Content.ReadAsStreamAsync();
            var image = new MagickImage(imageStream);

            return image;
        }

        public static async Task<ComponentBuilder> GetInterfaceButtons(BobiiInteractionContext context, ulong creatorChannelId)
        {
            var componentBuilder = new ComponentBuilder();
            var rowBuilder = new ActionRowBuilder();
            var rowCount = 0;

            foreach (var commandName in await GetTempCommands(context))
            {
                var commandUeberspringen = await TempCommandRepository.CommandDisabled(context.Guild!.Id, creatorChannelId, commandName) ||
                                           commandName == SlashCommandNames.Interface;

                if (commandUeberspringen)
                {
                    continue;
                }

                rowCount++;
                var emoteId = await InterfaceInformationsRepository.GetInterfaceEmoteIdWithFallback(context.Guild.Id, commandName);
                var button = ButtonHelper.GetInterfaceButton(commandName, emoteId);
                rowBuilder.WithButton(await button);

                if (rowCount != 4)
                {
                    continue;
                }

                rowCount = 0;
                componentBuilder.AddRow(rowBuilder);
                rowBuilder = new ActionRowBuilder();
            }

            if (rowBuilder != new ActionRowBuilder())
            {
                componentBuilder.AddRow(rowBuilder);
            }

            return componentBuilder;
        }
        #endregion

        #region Private Functions
        private static async Task<List<MagickImage>> GetCommandInfoImages(BobiiInteractionContext context, string[] commandNames)
        {
            var supportedCharacters = GetSupportedCharacters(GetCommandNameFontFilePath());
            var commandImages = new List<MagickImage>();

            foreach (var commandName in commandNames)
            {
                var image = await GetCommandInfoImage(
                    context,
                    commandName,
                    MagickColor.FromRgba(79, 79, 79, 255),
                    supportedCharacters);

                commandImages.Add(image);
            }

            return commandImages;
        }

        private static void AddCommandInfoImagesToInterfaceImage(MagickImage interfaceImg, List<MagickImage> commandImages)
        {
            var x = 0;
            var y = 0;
            var count = 0;
            foreach (var image in commandImages)
            {
                count++;
                interfaceImg.Composite(image, x, y, CompositeOperator.Over);
                image.Dispose();
                x += 215;

                if (count != 4)
                {
                    continue;
                }

                count = 0;
                y += 90;
                x = 0;
            }
        }

        private static async Task<MagickImage> GetCommandInfoImage(
            BobiiInteractionContext context,
            string commandName,
            MagickColor backgroundColor,
            string supportedCharacters)
        {
            var commandInfoImage = new MagickImage(MagickColors.Transparent, 200, 60);

            await AddBackgroundColor(context, commandInfoImage, commandName);
            await AddCommandImage(context, commandName, commandInfoImage);
            await AddCommandText(context, commandName, commandInfoImage, supportedCharacters);

            return commandInfoImage;
        }

        private static async Task AddBackgroundColor(BobiiInteractionContext context, MagickImage commandInfoImage, string commandName)
        {
            var customCommandColor = await InterfaceInformationsRepository.GetInterfaceCustomCommandColorWithFallback(context.Guild!.Id, commandName);
            var customCommandColorRgba = customCommandColor.Split(";");
            var rot = customCommandColorRgba[0].ToByte();
            var gruen = customCommandColorRgba[1].ToByte();
            var blau = customCommandColorRgba[2].ToByte();
            var alpha = customCommandColorRgba[3].ToByte();

            var graphics = new Drawables();
            graphics.RoundRectangle(0, 0, 200, 60, 10, 10);
            graphics.FillColor(MagickColor.FromRgba(rot, gruen, blau, alpha));
            graphics.Draw(commandInfoImage);
        }

        private static async Task AddCommandImage(BobiiInteractionContext context, string commandName, MagickImage commandInfoImage)
        {
            var commandImage = await GetCommandImage(context, commandName);
            commandImage.Resize(30, 30);
            commandInfoImage.Composite(commandImage, 12, 15, CompositeOperator.Over);
        }

        private static async Task AddCommandText(BobiiInteractionContext context, string commandName, MagickImage commandInfoImage, string supportedCharacters)
        {
            var customCommandName =await InterfaceInformationsRepository.GetInterfaceCustomCommandNameWithFallback(context.Guild!.Id, commandName);
            var font = Regex.IsMatch(customCommandName, $"^[{supportedCharacters} ]+$") ? GetCommandNameFontFilePath() : "Arial Bold";

            var drawables = new Drawables()
                .Font(font)
                .FontPointSize(23)
                .FillColor(MagickColors.White)
                .TextAlignment(TextAlignment.Left)
                .Text(49, 39, customCommandName);

            drawables.Draw(commandInfoImage);
        }

        private static string GetSupportedCharacters(string fontFilePath)
        {
            var typeface = SKTypeface.FromFile(fontFilePath);
            var font = new SKFont(typeface);
            var supportedCharacters = new StringBuilder();

            for (int i = 0; i <= char.MaxValue; i++)
            {
                var c = (char)i;
                if (IsCharacterSupported(c, font))
                {
                    supportedCharacters.Append(c);
                }
            }

            return supportedCharacters.ToString();
        }

        private static bool IsCharacterSupported(char c, SKFont font)
        {
            var glyphs = font.GetGlyphs(new[] { c });
            return glyphs.Length > 0 && glyphs[0] != 0; ;
        }

        private static string GetCommandNameFontFilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "CommandNameFont.ttf");
        }


        private static MagickImage GetMagickImage(int anzahlCommands)
        {
            var height = (anzahlCommands - 1) / 4 * 90 + 60;
            return new MagickImage(MagickColors.Transparent, 845, height);
        }

        private static async Task SaveInterfaceImage(MagickImage interfaceImg, BobiiInteractionContext context, ulong creatorChannelId)
        {
            Directory.CreateDirectory(GetInterfaceDirectory(context, creatorChannelId));
            await interfaceImg.WriteAsync(GetInterfaceFileName(context, creatorChannelId), MagickFormat.Png);
            interfaceImg.Dispose();
        }

        private static string GetInterfaceFileName(BobiiInteractionContext context, ulong creatorChannelId)
        {
            return Path.Combine(GetInterfaceDirectory(context, creatorChannelId), "interface.png");
        }

        private static string GetInterfaceDirectory(BobiiInteractionContext context, ulong creatorChannelId)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), context.Guild!.Id.ToString(), creatorChannelId.ToString());
        }

        private static async Task<string[]> GetTempCommands(BobiiInteractionContext context)
        {
            var commands = await context.Client!.GetGlobalApplicationCommandsAsync();
            return commands.Single(c => c.Name == SlashCommandNames.Temp)
                .Options
                .Select(c => c.Name)
                .ToArray();
        }
        #endregion
    }
}
