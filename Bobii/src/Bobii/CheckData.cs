using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii
{
    class CheckDatas
    {
        public static async Task<bool> CheckIfVoiceID(SocketInteraction interaction, string Id, string task, SocketGuild guild)
        {
            foreach (var channel in guild.VoiceChannels)
            {
                if (channel.Id.ToString() == Id)
                {
                    return false;
                }
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The given channel ID **'{Id}'** does not belong to a voice channel!\nMake sure to copy the voice Channel ID directly from the voice Channel!", "Invalid ID!").Result }, ephemeral: true);
            await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {Id} | Invalid ID");
            return true;
        }

        public static async Task<bool> CheckUserID(SocketInteraction interaction, string userId, SocketGuild guild, string task)
        {
            if (!ulong.TryParse(userId, out _) || userId.Length != 18)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The given channel ID **'{userId}'** is not valid!\nMake sure to copy the ID from the **user** directly!", "Invalid ID!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | UserID: {userId} | Invalid ID");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckDiscordChannelID(SocketInteraction interaction, string Id, SocketGuild guild, string task, bool channel)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(Id, out _) || Id.Length != 18)
            {
                if (channel)
                {
                    await interaction .RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The given channel ID **'{Id}'** is not valid!\nMake sure to copy the ID from the **voice channel** directly!", "Invalid ID!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {Id} | Invalid ID");
                    return true;
                }
                else
                {
                    await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The given guild ID **'{Id}'** is not valid!\nMake sure to copy the ID from the guild directly!", "Invalid ID!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | GuildID: {Id} | Invalid ID");
                    return true;
                }

            }
            return false;
        }

        public static async Task<bool> CheckDoubleCreateTempChannel(SocketInteraction interaction, string createChannelID, SocketGuild guild, string task)
        {
            if (TempChannel.EntityFramework.CreateTempChannelsHelper.CheckIfCreateVoiceChannelExist(guild, ulong.Parse(createChannelID)).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The create-temp-channel with the ID **'{createChannelID}'** already exists!\nYou can get a list of all create-temp-channels by using:\n**/tcinfo**", "Create-temp-channel exists already!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {createChannelID} | Double CreateTempChannel");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfCreateTempChannelExists(SocketInteraction interaction, string createChannelID, SocketGuild guild, string task)
        {
            if (!TempChannel.EntityFramework.CreateTempChannelsHelper.CheckIfCreateVoiceChannelExist(guild, ulong.Parse(createChannelID)).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The create-temp-channel with the ID **'{createChannelID}'** does not exists!\nYou can get a list of all create-temp-channels by using:\n**/tcinfo**", "Create-temp-channel does not exist!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {createChannelID} | CreateTempChannel does not exist");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfFilterWordDouble(SocketInteraction interaction, string filterWord, SocketGuild guild, string task)
        {
            if (FilterWord.EntityFramework.FilterWordsHelper.CheckIfFilterWordExists(guild.Id, filterWord).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The filter word with the name **'{filterWord}'** already exists!\nYou can get a list of all filter words by using:\n**/fwinfo**", "Filter word already exists!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | Filter word: {filterWord} | Double filter word");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfFilterWordExists(SocketInteraction interaction, string filterWord, SocketGuild guild, string task)
        {
            if (!FilterWord.EntityFramework.FilterWordsHelper.CheckIfFilterWordExists(guild.Id, filterWord).Result)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The Filter word with the name **'{filterWord}'** does not exists!\nYou can get a list of all filter words by using:\n**/fwinfo**", "Filter word does not exist!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | Filter word: {filterWord} | Filter word does not exist");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckNameLength(SocketInteraction interaction, string createChannelID, SocketGuild guild, string name, string task, int lenght, bool tempchannel)
        {
            if (name.Length > lenght)
            {
                if (tempchannel)
                {
                    await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The TempChannelName **'{name}'** has more than 50 characters, pls make sure the name is shorter than {lenght} characters!", "Too much characters!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | CreateChannelID: {createChannelID} | TempChannelName: {name} | Name has too much characters");
                    return true;
                }
                else
                {
                    await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The the word **'{name}'** has more than {lenght} characters, pls make sure the name is shorter than {lenght} characters!", "Too much characters!").Result }, ephemeral: true);
                    await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | Name has too much characters");
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> CheckUserPermission(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, SocketSlashCommandData parsedArg, string task)
        {
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild)
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Name}**\nYou need one fo the following permissions to use this command:\n**Administrator**\n**Manage Server**", "Missing permissions!").Result }, ephemeral: true);
            await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | User: {user}| Missing premissions");
            return true;
        }

        public static async Task<bool> CheckMessageID(SocketInteraction interaction, SocketGuild guild, string id, string task, DiscordSocketClient client)
        {
            if (!ulong.TryParse(id, out _) || id.Length != 18)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The given message ID **'{id}'** is not valid!\nMake sure to copy the ID from the **message wich includes the embed** directly!", "Invalid ID!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | MessageID: {id} | Invalid ID");
                return true;
            }

            var channel = (SocketTextChannel)client.GetChannel(interaction.Channel.Id);
            
            var messagesInChannel = channel.GetMessagesAsync(100).Flatten();
            if (messagesInChannel == null || messagesInChannel.ToArrayAsync().Result.Count()== 0)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"There are no messages in the channel which you just used the command in.\nUse this command in the chanenl with the messages which contains the embed you want to change!", "No messages detected!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | MessageID: {id} | Invalid ID");
                return true;
            }

            var message = messagesInChannel.ToArrayAsync().Result.Where(m => m.Id == ulong.Parse(id)).FirstOrDefault();
            if (message == null)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"There is no message in the channel in which you used this command with the given id **{id}**.\nPlease use this command in the channel which contains the message with the embed!", "Invalid ID!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | MessageID: {id} | Invalid ID");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckIfMessageFromCreateEmbed(SocketInteraction interaction, SocketGuild guild, ulong messageId, string task,DiscordSocketClient client)
        {
            var channel = (SocketTextChannel)client.GetChannel(interaction.Channel.Id);
            var messagesInChannel = channel.GetMessagesAsync(100).Flatten();
            var message = messagesInChannel.ToArrayAsync().Result.Where(m => m.Id == messageId).FirstOrDefault();
            // §TODO JG/220.11.2021 Check if this works
            if (!message.Author.IsBot || !(message.Author.Id == 776028262740393985 || message.Author.Id == 869180143363584060))
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The given message id does not belong to a message from Bobii!", "Not from Bobii!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | MessageID: {messageId} | Message from Bobii");
                return true;
            }

            if (message.Reference != null)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"This message was not created with /tucreatetembed!", "Not created with create embed!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | MessageID: {messageId} | Message from Bobii");
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckStringLength(SocketInteraction interaction, SocketGuild guild, string stringToCheck, int maxLenth, string parameterName, string task)
        {
            if (stringToCheck.Length < maxLenth)
            {
                return false;
            }
            await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"The length of **{parameterName}** cannot be longer than {maxLenth}!", "Invalid length!").Result }, ephemeral: true);
            await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | Parameter: {parameterName} | Invalid length of parameter");
            return true;
        }

        public static async Task<bool> CheckIfItsBobSty(SocketInteraction interaction, SocketGuild guild, SocketGuildUser user, SocketSlashCommandData parsedArg, string task, bool errorMessage)
        {
            //False = Its me
            //True = Its not me
            if (user.Id.ToString() == (410312323409117185).ToString())
            {
                return false;
            }
            if (errorMessage)
            {
                await interaction.RespondAsync(null, new Embed[] { Bobii.Helper.CreateEmbed(interaction, $"You dont have the permissions to use:\n**/{parsedArg.Name}**\n**__Only BobSty himselfe is allowed to use this command!__**", "Missing permissions!").Result }, ephemeral: true);
                await Handler.SlashCommandHandlingService.WriteToConsol($"Error: {guild.Name} | Task: {task} | Guild: {guild.Id} | User: {user} | Tryed to delete command: {Handler.SlashCommandHandlingService.GetOptions(parsedArg.Options).Result[0].Value} | Someone tryed to be Me");
            }
            return true;
        }
    }
}
