using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bobii.src.SCF
{
    public class scraper
    {
        private string _url;
        private DateTime _endTime;
        private ulong _channelID;
        private DiscordSocketClient _client;

        public scraper(DiscordSocketClient client, string ulr, ulong channelId, DateTime endTime) 
        { 
            _url = ulr;
            _endTime = endTime;
            _channelID = channelId;
            _client = client;
        }

        public void Start()
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Scraper     started");
            try
            {
                var channel = (SocketTextChannel)_client.GetChannel(_channelID);
                if (channel == null)
                {
                    Console.WriteLine("Channel could not be found");
                    return;
                }

                bool ticketsVerfuegbar = false;
                var httpClient = new HttpClient();

                while (DateTime.Now < _endTime)
                {

                    var websiteString = httpClient.GetStringAsync(_url).Result;
                    // Check if it contains "No tickets available"
                    if (websiteString.Contains("Aktuell keine Tickets verfügbar."))
                    {
                        ticketsVerfuegbar = false;
                    }
                    else
                    {
                        if (ticketsVerfuegbar)
                        {
                            // 60 seconds
                            Thread.Sleep(30000);
                            continue;
                        }
                        ticketsVerfuegbar = true;
                        channel.SendMessageAsync($"<@!410312323409117185> Es sind Tickets verfügbar!\n{_url}");
                    }

                    Console.WriteLine("5 seks warten:");
                    // Every 15 seconds
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Scraper ran into an error:" + ex.Message);
            }
        }

    }
}
