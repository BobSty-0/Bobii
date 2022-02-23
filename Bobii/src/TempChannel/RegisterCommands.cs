using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel
{
    class RegisterCommands
    {
        #region Info
        public static async Task Info(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcinfo")
                .WithDescription("Returns a list of all the create-temp-channels of this Guild")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelInfo", exceptionMessage: ex.Message);
            }
        }

        public static async Task CreateInfoForTempCommands(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tccreateinfo")
                .WithDescription("Creates an embed which shows all the commands to edit temp-channels")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "CreateInfoForTempCommands", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region Utility
        public static async Task Add(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcadd")
                .WithDescription("Adds an create-temp-channel")
                .AddOption("createvoicechannel", ApplicationCommandOptionType.String, "Choose the channel which you want to add as create-voice-channel", true, isAutocomplete: true)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("tempchannelname")
                    .WithDescription("This will be the name of the temp-channel. Note: {username} = Username")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channelsize")
                    .WithDescription("This will be the size of the temp-channel (OPTIONAL)")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("textchannel")
                    .WithDescription("Bobii will create an additional temp-text-channel if activated (OPTIONAL)")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.String)
                    .AddChoice("on", "on")
                    .AddChoice("off", "off"))
                //.AddOption(new SlashCommandOptionBuilder()
                //    .WithName("delay")
                //    .WithDescription("This will set the delete delay of the temp-channel (OPTIONAL)")
                //    .WithRequired(false)
                //    .WithType(ApplicationCommandOptionType.Integer))
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChanneAdd", exceptionMessage: ex.Message);
            }
        }

        public static async Task Update(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcupdate")
                .WithDescription("Updates the temp-channel name or/and size or/and textchannel of an existing create-temp-channel")
                .AddOption("createvoicechannel", ApplicationCommandOptionType.String, "Choose the channel which you want to update", true, isAutocomplete: true)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("newtempchannelname")
                    .WithDescription("This will be the new name of the temp-channel")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("newtempchannelsize")
                    .WithDescription("This will be the new size of the temp-channel")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("textchannel")
                    .WithDescription("Bobii will create an additional temp-text-channel if activated")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.String)
                    .AddChoice("on", "on")
                    .AddChoice("off", "off"))
               //.AddOption(new SlashCommandOptionBuilder()
               //     .WithName("delay")
               //     .WithDescription("This will set the new delete delay of the temp-channel")
               //     .WithRequired(false)
               //     .WithType(ApplicationCommandOptionType.Integer))
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelUpdate", exceptionMessage: ex.Message);
            }
        }

        public static async Task Remove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcremove")
                .WithDescription("Removes an create-temp-channel")
                .AddOption("createvoicechannel", ApplicationCommandOptionType.String, "Choose the channel which you want to remove", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelRemove", exceptionMessage: ex.Message);
            }
        }
        #endregion

        #region ChannelEdit
        public static async Task Name(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tempname")
                .WithDescription("Changes the name of the temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("newname")
                    .WithDescription("This will be the new name of the temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelName", exceptionMessage: ex.Message);
            }
        }

        public static async Task Size(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tempsize")
                .WithDescription("Changes the size of the temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("newsize")
                    .WithDescription("This will be the new size of the temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer))
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelSize", exceptionMessage: ex.Message);
            }
        }

        public static async Task Owner(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tempowner")
                .WithDescription("Transfers the ownership of the temp-channel")
                .AddOption("newowner", ApplicationCommandOptionType.String, "Choose the user you want to give owner to", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelOwner", exceptionMessage: ex.Message);
            }
        }

        public static async Task Kick(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tempkick")
                .WithDescription("Kicks a user from the temp-channel")
                .AddOption("user", ApplicationCommandOptionType.String, "Choose the user which you want to kick", true, isAutocomplete: true)
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelKick", exceptionMessage: ex.Message);
            }
        }

        public static async Task Block(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tempblock")
                .WithDescription("Blocks a user from the temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("@ the user which you want to block here @user")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.User))
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelBlock", exceptionMessage: ex.Message);
            }
        }

        public static async Task UnBlock(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tempunblock")
                .WithDescription("Unblocks a user from the temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("@ the user which you want to unblock here @user")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.User))
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelUnBlock", exceptionMessage: ex.Message);
            }
        }

        public static async Task Hide(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("temphide")
                .WithDescription("Hides the channels so nobody can see it")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelHide", exceptionMessage: ex.Message);
            }
        }

        public static async Task UnHide(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tempunhide")
                .WithDescription("Unhides the channel")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelHide", exceptionMessage: ex.Message);
            }
        }

        public static async Task Lock(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("templock")
                .WithDescription("Locks up the channels so nobody can join")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelLock", exceptionMessage: ex.Message);
            }
        }

        public static async Task UnLock(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tempunlock")
                .WithDescription("Unlocks the channels so people can join again")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (Exception ex)
            {
                await Handler.HandlingService._bobiiHelper.WriteToConsol("SCommRegis", true, "TempChannelUnLock", exceptionMessage: ex.Message);
            }
        }
        #endregion
    }
}
