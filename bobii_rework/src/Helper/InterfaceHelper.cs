
using bobii_rework.Entities.EntityFramework;
using bobii_rework.Entities.Interactions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.Repositories;
using bobii_rework.src.Repositories;
using Discord;
using ImageMagick;
using ImageMagick.Drawing;
using SkiaSharp;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using File = bobii_rework.Entities.EntityFramework.File;

namespace bobii_rework.Helper
{
    public static class InterfaceHelper
    {
        #region Constants
        private const string InterfaceFileName = "interface.webp";
        #endregion

        #region Methods
        private static async Task<List<InterfaceInformation>> GetInterfaceInformations(ulong guildId)
        {
            var guildCommandInfos = await InterfaceInformationsRepository.GetCustomGuildInterfaceInformations(guildId);
            var defaultCommandInfos = await InterfaceInformationsRepository.GetCustomGuildInterfaceInformations(Configuration.GetConfigValue<ulong>(Configuration.SupportGuildID));

            if (!guildCommandInfos.Any()) return defaultCommandInfos;

            foreach (var guildCommandInformation in guildCommandInfos)
            {
                var index = defaultCommandInfos.FindIndex(d => d.CommandName == guildCommandInformation.CommandName);
                if (index >= 0)
                {
                    // Überschreibe existierendes Element
                    defaultCommandInfos[index] = guildCommandInformation;
                }
            }

            return defaultCommandInfos;
        }

        public static async Task<File> CreateInterfaceImg(BobiiInteractionContext context, ulong creatorChannelId)
        {
            var disabledCommands = await TempCommandRepository.GetDisabledTempCommandNames(creatorChannelId);
            var interfaceInformations = await GetInterfaceInformations(context.Guild!.Id);

            interfaceInformations = interfaceInformations
                .Where(i => !disabledCommands.Contains(i.CommandName))
                .ToList();

            var enabledCommandNames = interfaceInformations
                .Select(i => i.CommandName)
                .ToArray();

            var commandsCombined = string.Join("|", enabledCommandNames);
            using var sha256 = SHA256.Create();
            var fileHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(commandsCombined));

            var file = await FileRepository.GetFile(fileHash);
            if (file != null) return file;

            var interfaceImg = GetMagickImage(interfaceInformations.Count());
            var commandInfoImages = await GetCommandInfoImages(context, interfaceInformations);

            AddCommandInfoImagesToInterfaceImage(interfaceImg, commandInfoImages);

            interfaceImg.Format = MagickFormat.WebP;
            file = await FileRepository.CreateFile(interfaceImg.ToByteArray(), fileHash);

            interfaceImg.Dispose();
            return file;
        }

        public static async Task<MagickImage> GetCommandImage(InterfaceInformation interfaceInformation)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://cdn.discordapp.com/emojis/{interfaceInformation.EmoteId}.webp");
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

            var interfaceInformations = await GetInterfaceInformations(context.Guild!.Id);

            // TODO hier die Sort mit einbauen aus der Datenbank
            foreach (var interfaceInformation in interfaceInformations)
            {
                var commandUeberspringen = await TempCommandRepository.CommandDisabled(context.Guild!.Id, creatorChannelId, interfaceInformation.CommandName);

                if (commandUeberspringen)
                {
                    continue;
                }

                rowCount++;
                var button = ButtonHelper.GetInterfaceButton(interfaceInformation.CommandName, interfaceInformation.EmoteId);
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
        private static async Task<List<MagickImage>> GetCommandInfoImages(BobiiInteractionContext context, List<InterfaceInformation> interfaceInformations)
        {
            var supportedCharacters = GetSupportedCharacters(GetCommandNameFontFilePath());
            var commandImages = new List<MagickImage>();

            foreach (var interfaceInformation in interfaceInformations)
            {
                var image = await GetCommandInfoImage(
                    interfaceInformation,
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
                y += 80;
                x = 0;
            }
        }

        private static async Task<MagickImage> GetCommandInfoImage(
            InterfaceInformation interfaceInformation,
            string supportedCharacters)
        {
            var commandInfoImage = new MagickImage(MagickColors.Transparent, 200, 60);

            await AddBackgroundColor(commandInfoImage, interfaceInformation);
            await AddCommandImage(commandInfoImage, interfaceInformation);
            await AddCommandText(commandInfoImage, interfaceInformation, supportedCharacters);

            return commandInfoImage;
        }

        private static async Task AddBackgroundColor(MagickImage commandInfoImage, InterfaceInformation interfaceInformation)
        {
            var graphics = new Drawables();
            graphics.RoundRectangle(0, 0, 200, 60, 10, 10);
            graphics.FillColor(new MagickColor(interfaceInformation.CustomCommandBackgroundColorHex));
            graphics.Draw(commandInfoImage);
        }

        private static async Task AddCommandImage(MagickImage commandInfoImage, InterfaceInformation interfaceInformation)
        {
            var commandImage = await GetCommandImage(interfaceInformation);
            commandImage.Resize(30, 30);
            commandInfoImage.Composite(commandImage, 12, 15, CompositeOperator.Over);
        }

        private static async Task AddCommandText(MagickImage commandInfoImage, InterfaceInformation interfaceInformation, string supportedCharacters)
        {
            var font = Regex.IsMatch(interfaceInformation.CustomCommandName, $"^[{supportedCharacters} ]+$") ? GetCommandNameFontFilePath() : "Arial Bold";

            var drawables = new Drawables()
                .Font(font)
                .FontPointSize(23)
                .FillColor(new MagickColor(interfaceInformation.CustomCommandForeColorHex))
                .TextAlignment(TextAlignment.Left)
                .Text(49, 39, interfaceInformation.CustomCommandName);

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
            // TODO schauen ob das hier wirklich so funktioniert
            return Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "CommandNameFont.ttf");
        }


        private static MagickImage GetMagickImage(int anzahlCommands)
        {
            var height = (uint)(anzahlCommands - 1) / 4 * 80 + 60;
            var settings = new MagickReadSettings
            {
                Width = 845,
                Height = height,
                BackgroundColor = MagickColors.Transparent
            };
            return new MagickImage("xc:none", settings);
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
