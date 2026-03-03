using bobii_rework.Entities.EntityFramework;
using Microsoft.EntityFrameworkCore;
using File = bobii_rework.Entities.EntityFramework.File;

namespace bobii_rework.EntityFramework
{
    class BobiiContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.GetConfigValue<string>(Configuration.ConnectionString));
        }

        public DbSet<TempChannelUserConfig> TempChannelUserConfigs { get; set; }
        public DbSet<CreateTempChannel> CreateTempChannels { get; set; }
        public DbSet<TempChannel> TempChannels { get; set; }
        public DbSet<TempCommand> Commands { get; set; }
        public DbSet<UsedFunction> UsedFunctions { get; set; }
        public DbSet<InterfaceInformation> InterfaceInformations { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Emote> Emotes { get; set; }
    }

    class BobiiLngContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.GetConfigValue<string>(Configuration.ConnectionStringLng));
        }

        public DbSet<Caption> Captions { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Command> Commands { get; set; }
    }
}
