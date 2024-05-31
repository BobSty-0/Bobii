using bobii_rework.Entities.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace bobii_rework.EntityFramework
{
    class BobiiContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.GetConfigValue<string>(Configuration.ConnectionString));
        }

        public DbSet<tempchanneluserconfig> TempChannelUserConfigs { get; set; }
        public DbSet<createtempchannels> CreateTempChannels { get; set; }
        public DbSet<tempchannels> TempChannels { get; set; }
        public DbSet<tempcommands> Commands { get; set; }
        public DbSet<usedfunctions> UsedFunctions { get; set; }
        public DbSet<autoscalecategory> AutoScaleCategories { get; set; }
    }

    class BobiiLngContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.GetConfigValue<string>(Configuration.ConnectionStringLng));
        }

        public DbSet<caption> Captions { get; set; }
        public DbSet<content> Contents { get; set; }
        public DbSet<language> Languages { get; set; }
        public DbSet<commands> Commands { get; set; }
    }
}
