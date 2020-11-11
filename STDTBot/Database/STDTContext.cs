using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using STDTBot.Models;

namespace STDTBot.Database
{
    public class STDTContext : DbContext
    {
        internal DbSet<RaidAttendee> RaidAttendees { get; set; }
        internal DbSet<RaidInfo> Raids { get; set; }
        internal DbSet<RankInfo> Ranks { get; set; }
        internal DbSet<Referral> Referrals { get; set; }
        internal DbSet<SpecialChannel> SpecialChannels { get; set; }
        internal DbSet<User> Users { get; set; }

        public STDTContext(DbContextOptions<STDTContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("STDT_Users");
            modelBuilder.Entity<User>().HasKey("ID");

            modelBuilder.Entity<RaidAttendee>().ToTable("STDT_RaidAttendees");
            modelBuilder.Entity<RaidAttendee>().HasKey("UserID", "RaidID");

            modelBuilder.Entity<RaidInfo>().ToTable("STDT_Raids");
            modelBuilder.Entity<RaidInfo>().HasKey("RaidID");

            modelBuilder.Entity<RankHistory>().ToTable("STDT_RankHistory");
            modelBuilder.Entity<RankHistory>().HasKey("ID");

            modelBuilder.Entity<RankInfo>().ToTable("STDT_Ranks");
            modelBuilder.Entity<RankInfo>().HasKey("ID");

            modelBuilder.Entity<Referral>().ToTable("STDT_Referrals");
            modelBuilder.Entity<Referral>().HasKey("ID");

            modelBuilder.Entity<SpecialChannel>().ToTable("STDT_Channels");
            modelBuilder.Entity<SpecialChannel>().HasKey("ChannelID");
        }
    }
}
