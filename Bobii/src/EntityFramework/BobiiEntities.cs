using Bobii.src.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Bobii.src.Helper;

namespace Bobii.src.EntityFramework
{
    class BobiiEntities : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = GeneralHelper.GetConfigKeyValue(Bobii.ConfigKeys.ConnectionString);
            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<tempchanneluserconfig> TempChannelUserConfigs { get; set; }
        public DbSet<createtempchannels> CreateTempChannels { get; set; }
        public DbSet<tempchannels> TempChannels { get; set; }
        public DbSet<tempcommands> Commands { get; set; }
        public DbSet<usedfunctions> UsedFunctions { get; set; }
    }

    class BobiiLngCodes : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = GeneralHelper.GetConfigKeyValue(Bobii.ConfigKeys.ConnectionStringLng);
            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<caption> Captions { get; set; }
        public DbSet<content> Contents { get; set; }
        public DbSet<language> Languages { get; set; }
        public DbSet<commands> Commands { get; set; }
    }
}
