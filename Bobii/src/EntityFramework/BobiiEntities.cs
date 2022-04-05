﻿using Bobii.src.EntityFramework.Entities;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace Bobii.src.EntityFramework
{
    class BobiiEntities : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = Bobii.Helper.ReadBobiiConfig(Bobii.ConfigKeys.ConnectionString);
            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<createtempchannels> CreateTempChannels { get; set; }
        public DbSet<filterlink> FilterLink { get; set; }
        public DbSet<filterlinklogs> FilterLinkLogs { get; set; }
        public DbSet<filterlinkoptions> FilterLinkOptions { get; set; }
        public DbSet<filterlinksguild> FilterLinksGuild { get; set; }
        public DbSet<filterlinkuserguild> FilterLinkUserGuild { get; set; }
        public DbSet<filterwords> FilterWords { get; set; }
        public DbSet<tempchannels> TempChannels { get; set; }
        public DbSet<language> Languages { get; set; }
    }

    class BobiiLngCodes : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = Bobii.Helper.ReadBobiiConfig(Bobii.ConfigKeys.ConnectionStringLng);
            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<caption> Captions { get; set; }
        public DbSet<content> Contents { get; set; }
        public DbSet<language> Languages { get; set; }
        public DbSet<commands> Commands { get; set; }
    }
}
