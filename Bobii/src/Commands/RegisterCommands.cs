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
        public static async Task RegisterBadWordInfoCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "badwordinfo",
                Description = "Returns all the BadWords of this Guild",
            });
        }

        public static async Task RegisterBadWordAddCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "badwordadd",
                Description = "Adds a BadWord",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "badword",
                        Required = true,
                        Description = "The BadWord which should be replaced",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "replaceword",
                        Required = true,
                        Description = "The word which replaces the BadWord",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });
        }

        public static async Task RegisterBadWordRemoveCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "badwordremove",
                Description = "Removes a BadWord",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "badword",
                        Required = true,
                        Description = "The BadWord to remove",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },
                }
            });
        }

        public static async Task RegisterBadWordUpdateCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "badwordchangereplaceword",
                Description = "Changes the ReplaceWord of the given BadWord",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "badword",
                        Required = true,
                        Description = "The BadWord to update",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "newreplaceword",
                        Required = true,
                        Description = "The new ReplaceWord",
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
                Name = "tempinfo",
                Description = "Returns all the TempChannels of this Guild",
            });
        }

        public static async Task RegisterTempAddCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempadd",
                Description = "Adds an CreateTempChannel",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "channelid",
                        Required = true,
                        Description = "ID of the CreateTempChannel",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "tempchannelname",
                        Required = true,
                        Description = "This will be the name of the TempChannel. Note: User = Username",
                        Type = Discord.ApplicationCommandOptionType.String,
                    }
                }
            });
        }

        public static async Task RegisterTempRemoveCommand(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempremove",
                Description = "Removes an CreateTempChannel",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "channelid",
                        Required = true,
                        Description = "ID of the CreateTempChannel",
                        Type = Discord.ApplicationCommandOptionType.String
                    },
                }
            });
        }

        public static async Task RegisterTempChangeName(DiscordSocketClient client)
        {
            await client.Rest.CreateGlobalCommand(new Discord.SlashCommandCreationProperties()
            {
                Name = "tempchangename",
                Description = "Changes the TempChannel name fo an already existing CreateTempChannel",
                Options = new List<Discord.ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "channelid",
                        Required = true,
                        Description = "ID of the CreateTempChannel",
                        Type = Discord.ApplicationCommandOptionType.String,
                    },

                    new ApplicationCommandOptionProperties()
                    {
                        Name = "tempchannelname",
                        Required = true,
                        Description = "This will be the new name of the TempChannel",
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
