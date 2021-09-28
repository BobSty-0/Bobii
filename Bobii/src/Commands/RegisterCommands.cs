using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    class RegisterCommands
    {
        #region Declarations
        private static ulong _myGuildID = 712373862179930144;
        #endregion

        #region Methods
        public static async void WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} SComRegi    {message}");
            await Task.CompletedTask;
        }
        #endregion

        #region Register Tasks
        public static async Task RegisterFilterLinkLogSet(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("logset")
                .WithDescription("Sets the given channel to the filter link log")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithDescription("# the channel which you want set as filter link log")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterLinkLogUpdate(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("logupdate")
                .WithDescription("Updates the log channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithDescription("# the new channel which you want set as filter link log")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterLinkLogRemove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("logremove")
                .WithDescription("Removes the filter link log")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterLinkRemoveUser(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fluremove")
                .WithDescription("Removes an user from the filter link whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("@ the user which you want to remove from the whitelist")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterLinkAddUser(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fluadd")
                .WithDescription("Adds an user to the filter link whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithDescription("@ the user which you want to add to the whitelist")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFliterLinkInfo(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flinfo")
                .WithDescription("Returns a list of links/Users on the whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("usersorlinks")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer)
                    .AddChoice("Whitelisted links", 1)
                    .AddChoice("Whitelisted users", 2)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterLinkWhitelistRemove(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fllremove")
                .WithDescription("Removes an link of the whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("link")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                    .AddChoice("youtube", "YouTube")
                    .AddChoice("twitter", "Twitter")
                    .AddChoice("twitch", "Twitch")
                    .AddChoice("steam", "Steam")
                    .AddChoice("reddit", "Reddit")
                    .AddChoice("instagram", "Instagram")
                    .AddChoice("stackoverflow", "Stackoverflow")
                    .AddChoice("discord", "Discord")
                    .AddChoice("github", "Github")
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterLinkWhitelistAdd(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flladd")
                .WithDescription("Adds an link to the whitelist")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("link")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                    .AddChoice("youtube", "YouTube")
                    .AddChoice("twitter", "Twitter")
                    .AddChoice("twitch", "Twitch")
                    .AddChoice("steam", "Steam")
                    .AddChoice("reddit", "Reddit")
                    .AddChoice("instagram", "Instagram")
                    .AddChoice("stackoverflow", "Stackoverflow")
                    .AddChoice("discord", "Discord")
                    .AddChoice("github", "Github")
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterLinkSet(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("flset")
                .WithDescription("Activates or deactivates filterlink")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("state")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer)
                    .AddChoice("active", 1)
                    .AddChoice("inactive", 2))
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterMusicPlay(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("mplay")
                .WithDescription("Plays the music of the given Link")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("link")
                    .WithDescription("link of the song which should be played")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterRustGetServer(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("rgetserver")
                .WithDescription("Gets a list of Rust server")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("multiplicator")
                    .WithDescription("Chose from the given choices")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer)
                    .AddChoice("Vanilla", 1)
                    .AddChoice("2x", 2)
                    .AddChoice("3x", 3))  
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }
        
        public static async Task RegisterTestHelp(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("testhelp")
                .WithDescription("Help to test some things")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }
        
        public static async Task RegisterFilterWordInfoCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwinfo")
                .WithDescription("Returns a list of all the filter words of this Guild")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterWordAddCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwadd")
                .WithDescription("Adds a filter word")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("filterword")
                    .WithDescription("The filter word which should be replaced")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("replaceword")
                    .WithDescription("The word with which the filtered word should be replaced with")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterWordRemoveCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwremove")
                .WithDescription("Removes a filter word")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("filterword")
                    .WithDescription("The filer word which should be removed")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterFilterWordUpdateCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("fwupdate")
                .WithDescription("Updates the word which will replace the filter word")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("filterword")
                    .WithDescription("The filter word to update")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("newreplaceword")
                    .WithDescription("The new word which will replace the filter word")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterBobiiGuidesCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("bobiiguides")
                .WithDescription("Returns all my guides for a better understanding of Bobii")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterHelpCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("helpbobii")
                .WithDescription("Returns a list of all my Commands")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterTempInfoCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcinfo")
                .WithDescription("Returns a list of all the create-temp-channels of this Guild")
                .Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterTempAddCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcadd")
                .WithDescription("Adds an create-temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("voicechannelid")
                    .WithDescription("ID of the create-temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("tempchannelname")
                    .WithDescription("This will be the name of the temp-channel. Note: User = Username")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterTempRemoveCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcremove")
                .WithDescription("Removes an create-temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("voicechannelid")
                    .WithDescription("ID of the create-temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterTempUpdate(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
                .WithName("tcupdate")
                .WithDescription("Updates the temp-channel name of an existing create-temp-channel")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("voicechannelid")
                    .WithDescription("ID of the create-temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("newtempchannelname")
                    .WithDescription("This will be the new name of the temp-channel")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).Build();

            try
            {
                await client.Rest.CreateGlobalCommand(command);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterComRegisterCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comregister")
            .WithDescription("Registers a slashcommand")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be registered")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterComRemoveGuildCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comdeleteguild")
            .WithDescription("Removes a slashcommand from a guild")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be removed")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("guildid")
                .WithDescription("The guild in wich the command to delete is")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task RegisterComRemoveCommand(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comdelete")
            .WithDescription("Removes a slashcommand")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be removed")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                WriteToConsol($"Error | {ex.Message}");
            }
        }
        #endregion
    }
}
