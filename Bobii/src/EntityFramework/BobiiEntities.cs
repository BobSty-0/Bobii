using Bobii.src.EntityFramework.Entities;
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

        public DbSet<tempchanneluserconfig> TempChannelUserConfigs { get; set; }
        public DbSet<createtempchannels> CreateTempChannels { get; set; }
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
