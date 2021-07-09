using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bobii.src.Commands
{
    class SlashCommands
    {
        public static async Task SlashCommandHandler(SocketInteraction interaction, DiscordSocketClient client)
        {
            var parsedArg = (SocketSlashCommand)interaction;
            var user = (SocketGuildUser)parsedArg.User;
            var guildID = TextChannel.TextChannel.GetGuildWithInteraction(interaction).Id.ToString();
            var guild = TextChannel.TextChannel.GetGuildWithInteraction(interaction);

            switch (parsedArg.Data.Name)
            {
                case "tempinfo":
                    await interaction.RespondAsync("", false, TempVoiceChannel.TempVoiceChannel.CreateVoiceChatInfoEmbed(guildID, client, interaction));
                    break;
                case "help":
                    await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateHelpInfoSlash(guildID, interaction, client));
                    break;
                case "tempadd":
                    await TempAdd(parsedArg, interaction, guildID, guild, user);
                    break;
            }
        }

        public static async Task WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} SCommands   {message}");
            await Task.CompletedTask;
        }

        private static async Task TempAdd(SocketSlashCommand parsedArg, SocketInteraction interaction, string guildID, SocketGuild guild, SocketUser user)
        {
            var optionList = new List<SocketSlashCommandDataOption>();

            //Extracting the options
            foreach (var option in parsedArg.Data.Options)
            {
                optionList.Add(option);
            }

            var createChannelId = optionList[0].Value.ToString();
            var name = optionList[1].Value.ToString();

            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(createChannelId, out _) && createChannelId.Length != 18)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The given ChannelID **'{createChannelId}'** is not valid!\nMake sure to copy the ID from the voice channel directly!", "Invalid ID!"));
                await WriteToConsol($"Error: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelId} | Invalid ID");
                return;
            }

            if (DBStuff.createtempchannels.CheckIfCreateVoiceChannelExist(guildID, createChannelId))
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel with the ID **'{createChannelId}'** already exists!\nYou can get a list of all CreateTempChannels with:\n**/tempinfo**", "CreateTempChannel exists already!"));
                await WriteToConsol($"Error: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelId} | Double CreateTempChannel");
                return;
            }

            if (name.Length > 50)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The TempChannelName **'{name}'** has more than 50 characters, pls make sure the name is shorter than 50 characters!", "Too much characters!"));
                await WriteToConsol($"Error: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelId} | Name has too much characters");
                return;
            }

            //Replaceing ' because of the SQL Query -> Need to get a better solution here
            name = name.Replace("'", "’");

            try
            {
                DBStuff.createtempchannels.AddCC(guildID, name, createChannelId);
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, $"The CreateTempChannel ** {guild.GetChannel(ulong.Parse(createChannelId)).Name}** was sucessfully added by **{user.Username}**","CreateTempChannel sucessfully added!"));
                await WriteToConsol($"Information: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelId} | Successfully added CreateTempChannel");
            }
            catch (Exception ex)
            {
                await interaction.RespondAsync(null, false, TextChannel.TextChannel.CreateEmbed(interaction, "**CreateTempChannel could not be added**", "Error!"));
                await WriteToConsol($"Error: | Task: TempAdd | Guild: {guildID} | CreateChannelID: {createChannelId} | Failed to add CreateTempChannel | {ex.Message}");
                throw;
            }
            
        }
    }
}
