using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    class RegisterCommands
    {
        #region Declarations
        private static ulong _myGuildID = 712373862179930144;
        #endregion

        #region Register Tasks 
        public static async Task RegisterFilterWordInfoCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "fwinfo",
                Description = "Returns all the filter words of this Guild",
            });
        }

        public static async Task RegisterFilterWordAddCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "fwadd",
                Description = "Adds a _filter word_ which will be replaced with the _replace word_",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "filterword",
                        Required = true,
                        Description = "The _filter word_ which should be replaced",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "replaceword",
                        Required = true,
                        Description = "The word with which the _filtered word_ should be replaced",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });
        }

        public static async Task RegisterFilterWordRemoveCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "fwremove",
                Description = "Removes a _filter word_",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "filterword",
                        Required = true,
                        Description = "The _filer word_ which should be removed",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },
                }
            });
        }

        public static async Task RegisterFilterWordUpdateCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "fwupdate",
                Description = "Updates the word which will replace the _filter word_",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "filterword",
                        Required = true,
                        Description = "The _filter word_ to update",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "newreplaceword",
                        Required = true,
                        Description = "The new word which will replace the _filter word_",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });
        }

        public static async Task RegisterHelpCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "helpbobii",
                Description = "Returns a list of all my Commands",
            });
        }

        public static async Task RegisterTempInfoCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tcinfo",
                Description = "Returns all the _create temp channels_ of this Guild",
            });
        }

        public static async Task RegisterTempAddCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tcadd",
                Description = "Adds an _create temp channel_",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "channelid",
                        Required = true,
                        Description = "ID of the _create temp channel_",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "tempchannelname",
                        Required = true,
                        Description = "This will be the name of the _temp channel_. Note: User = Username",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });
        }

        public static async Task RegisterTempRemoveCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tcremove",
                Description = "Removes an _create temp channel_",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "channelid",
                        Required = true,
                        Description = "ID of the _create temp channel_",
                        Type = Discord.ApplicationCommandOptionType.String
                    },
                }
            });
        }

        public static async Task RegisterTempUpdate(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tcupdate",
                Description = "Updates the _temp channel name_ of an existing _create temp channel_",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "channelid",
                        Required = true,
                        Description = "ID of the _create temp channel_",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "tempchannelname",
                        Required = true,
                        Description = "This will be the new name of the _temp channel_",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });
        }

        public static async Task RegisterComRegisterCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGuildCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "comregister",
                Description = "Registers a slashcommand",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "commandname",
                        Required = true,
                        Description = "The name oft he command which should be registered",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            }, _myGuildID);
        }

        public static async Task RegisterComRemoveGuildCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGuildCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "comdeleteguild",
                Description = "Removes a slashcommand from a guild",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "commandname",
                        Required = true,
                        Description = "The name oft he command which should be removed",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "guildid",
                        Required = true,
                        Description = "The guild in wich the command to delete is",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            }, _myGuildID);
        }

        public static async Task RegisterComRemoveCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGuildCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "comdelete",
                Description = "Removes a slashcommand",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "commandname",
                        Required = true,
                        Description = "The name oft he command which should be removed",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            }, _myGuildID);
        }
        #endregion

    }
}
