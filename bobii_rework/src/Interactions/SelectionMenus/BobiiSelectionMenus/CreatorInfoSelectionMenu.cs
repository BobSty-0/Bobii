using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using bobii_rework.Entities.EntityFramework;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Interactions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Repositories;
using bobii_rework.src.Components;
using bobii_rework.src.GlobalConstants.Interactions;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord.Interactions;

namespace bobii_rework.Interactions.SelectionMenus.BobiiSelectionMenus
{
    public class CreatorInfoSelectionMenu : BobiiInteractionBase
    {
        #region Declarations
        private readonly InteractionService _interactionService;
        private readonly ulong _channelId;

        private const int ColumnWidthCommands = 40;
        private const int ColumnWidthCommandState = 10;
        #endregion

        #region Constructor
        public CreatorInfoSelectionMenu(InteractionContext context, InteractionService interactionService, ulong channelId) : base(context)
        {
            _interactionService = interactionService;
            _channelId = channelId;
        }
        #endregion

        #region Tasks
        public override async Task ExecuteCommand()
        {
            var creatorChannel = await CreatorChannelRepository.GetCreatorChannel(_channelId);
            var disabledCommands = await TempCommandRepository.GetTempCommands(Context.Guild!.Id, _channelId);

            var informationFormatted = await GetInformationFormatted(creatorChannel, disabledCommands);

            await Context.ModifyOriginalResponseWithEmbedAsync(informationFormatted);
        }
        #endregion

        #region Private Functions
        private async Task<string> GetInformationFormatted(createtempchannels creatorChannel, List<tempcommands> disabledCommands)
        {
            var sb = new StringBuilder();

            await AddCreatorChannelName(sb, creatorChannel);

            await AddTempChannelName(sb, creatorChannel);
            await AddTempChannelSize(sb, creatorChannel);
            await AddTempChannelDelay(sb, creatorChannel);
            await AddTempChannelAutoDeleteTimer(sb, creatorChannel);

            sb.AppendLine();

            await AddTempChannelCommandsTable(sb, creatorChannel);
            await AddTempChannelSettingsTable(sb, creatorChannel);

            return sb.ToString();
        }

        private async Task AddTempChannelSettingsTable(StringBuilder sb, createtempchannels creatorChannel)
        {
            sb.AppendLine("```");

            await AddTableHeaderRow(sb, Captions.Utility, Captions.Enabled);
            await AddTableUtilityRows(sb, creatorChannel);

            sb.AppendLine("```");
        }

        private async Task AddTempChannelCommandsTable(StringBuilder sb, createtempchannels creatorChannel)
        {
            sb.AppendLine("```");

            await AddTableHeaderRow(sb, Captions.Command, Captions.Enabled);
            await AddTableCommandRows(sb, creatorChannel);

            sb.AppendLine("```");
        }

        private async Task AddTableHeaderRow(StringBuilder sb, string spcCommandCaption, string spcStatusCaption)
        {
            AddRowSeperator(sb, "╔", "╦", "╗");

            var commandCaption = await Context.GetCaptionAsync(spcCommandCaption);
            var statusCaption = await Context.GetCaptionAsync(spcStatusCaption);

            await AddTableRow(sb, commandCaption, statusCaption);
        }

        private async Task AddTableUtilityRows(StringBuilder sb, createtempchannels creatorChannel)
        {
            var utilities = GetUtilities();

            var count = 0;
            foreach (var utility in utilities)
            {
                count++;
                var utilityName = await Context.GetCaptionAsync(GetUtilitiesSprachcodes()[utility]);
                var disabled = await TempCommandRepository.CommandDisabled(Context.Guild!.Id, creatorChannel.createchannelid, utility);

                await AddTableRow(sb, utilityName, (!disabled).ToString(), count == utilities.Length);
            }
        }

        private Dictionary<string, string> GetUtilitiesSprachcodes()
        {
            return new Dictionary<string, string>()
            {
                { UtilityNames.Interface, Captions.Interface },
                { UtilityNames.OwnerPermissions, Captions.OwnerPermissions },
                { UtilityNames.KickBlockedUsersOnOwnerChange, Captions.KickBlockedUsersOnOwnerChange },
                { UtilityNames.HideVoiceFromBlockedUsers, Captions.HideVoiceFromBlockedUsers },
                { UtilityNames.AutoTransferOwner, Captions.AutoTransferOwner }
            };
        }

        private string[] GetUtilities()
        {
            return new[]
            {
                UtilityNames.Interface,
                UtilityNames.OwnerPermissions,
                UtilityNames.KickBlockedUsersOnOwnerChange,
                UtilityNames.HideVoiceFromBlockedUsers,
                UtilityNames.AutoTransferOwner
            };
        }

        private async Task AddTableCommandRows(StringBuilder sb, createtempchannels creatorChannel)
        {
            var commands = _interactionService.SlashCommands
                .Where(c => c.Module.SlashGroupName == SlashCommandNames.Temp)
                .Select(c => c.Name)
                .ToArray();

            var count = 0;
            foreach (var command in commands)
            {
                count++;
                var commandName = $"/temp {command}";
                var disabled = await TempCommandRepository.CommandDisabled(Context.Guild!.Id, creatorChannel.createchannelid, command);

                await AddTableRow(sb, commandName, (!disabled).ToString(), count == commands.Length);
            }
        }

        private async Task AddTableRow(StringBuilder sb, string command, string state, bool lastRow = false)
        {
            await AddRow(sb, command, state);

            if (lastRow)
            {
                AddRowSeperator(sb, "╚", "╩", "╝");
                return;
            }

            AddRowSeperator(sb, "╠", "╬", "╣");
        }

        public async Task AddRow(StringBuilder sb, string command, string status)
        {
            var sbRow = new StringBuilder();

            sbRow.Append("║ ");


            sbRow.Append(command);
            AppendString(sbRow, ColumnWidthCommands - command.Length - 1, " ");

            sbRow.Append("║ ");

            sbRow.Append(status);
            AppendString(sbRow, ColumnWidthCommandState - status.Length - 1, " ");

            sbRow.Append("║");

            sb.AppendLine(sbRow.ToString());
        }

        public void AppendString(StringBuilder sb, int count, string str)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }
        }

        private void AddRowSeperator(StringBuilder sb, string startSymbol, string trennSymbol, string endSymbol)
        {
            var sbRowTop = new StringBuilder();

            sbRowTop.Append(startSymbol);
            AppendString(sbRowTop, ColumnWidthCommands, "═");
            sbRowTop.Append(trennSymbol);
            AppendString(sbRowTop, ColumnWidthCommandState, "═");
            sbRowTop.Append(endSymbol);

            sb.AppendLine(sbRowTop.ToString());
        }

        private async Task AddCreatorChannelName(StringBuilder sb, createtempchannels creatorChannel)
        {
            var channel = await Context.Guild!.GetChannelAsync(creatorChannel.createchannelid);
            sb.AppendLine($"## {channel.Name}");
        }

        private async Task AddTempChannelName(StringBuilder sb, createtempchannels creatorChannel)
        {
            var tempChannelName = string.Format(
                await Context.GetContentAsync(Contents.TempChannelName),
                creatorChannel.tempchannelname);

            sb.AppendLine(tempChannelName);
        }

        private async Task AddTempChannelSize(StringBuilder sb, createtempchannels creatorChannel)
        {
            if (creatorChannel.channelsize.GetValueOrDefault() == 0)
            {
                return;
            }

            var tempChannelSize = string.Format(
                await Context.GetContentAsync(Contents.TempChannelSize),
                creatorChannel.channelsize);

            sb.AppendLine(tempChannelSize);
        }

        private async Task AddTempChannelDelay(StringBuilder sb, createtempchannels creatorChannel)
        {
            if (creatorChannel.delay.GetValueOrDefault() == 0)
            {
                return;
            }

            var tempChannelDelay = string.Format(
                await Context.GetContentAsync(Contents.TempChannelDelay),
                creatorChannel.delay);

            sb.AppendLine(tempChannelDelay);
        }

        private async Task AddTempChannelAutoDeleteTimer(StringBuilder sb, createtempchannels creatorChannel)
        {
            var commandDisabled = await TempCommandRepository.CommandDisabled(
                Context.Guild!.Id,
                _channelId,
                SlashCommandNames.Chat);

            if (commandDisabled || creatorChannel.autodelete.GetValueOrDefault() == 0)
            {
                return;
            }

            var tempChannelAutodelete = string.Format(
                await Context.GetContentAsync(Contents.TempChannelAutodelete),
                creatorChannel.autodelete);

            sb.AppendLine(tempChannelAutodelete);
        }
        #endregion
    }
}
