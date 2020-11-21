﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using STDTBot.Database;

namespace STDTBot.Migrations
{
    [DbContext(typeof(STDTContext))]
    [Migration("20201120234400_config")]
    partial class config
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("STDTBot.Models.CommandChannelPermission", b =>
                {
                    b.Property<string>("CommandName")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<long>("ChannelID")
                        .HasColumnType("bigint");

                    b.HasKey("CommandName", "ChannelID");

                    b.ToTable("STDT_Command_Channels");
                });

            modelBuilder.Entity("STDTBot.Models.CommandRolePermission", b =>
                {
                    b.Property<string>("CommandName")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<long>("MinimumRole")
                        .HasColumnType("bigint");

                    b.HasKey("CommandName");

                    b.ToTable("STDT_Command_Roles");
                });

            modelBuilder.Entity("STDTBot.Models.Config", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("STDT_Config");
                });

            modelBuilder.Entity("STDTBot.Models.MIData", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DateLogged")
                        .HasColumnType("datetime");

                    b.Property<int>("PointsLogged")
                        .HasColumnType("int");

                    b.Property<int>("ReasonLogged")
                        .HasColumnType("int");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.ToTable("STDT_MIData");
                });

            modelBuilder.Entity("STDTBot.Models.RaidAttendee", b =>
                {
                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.Property<long>("RaidID")
                        .HasColumnType("bigint");

                    b.Property<long>("MinutesInRaid")
                        .HasColumnType("bigint");

                    b.Property<long>("PointsObtained")
                        .HasColumnType("bigint");

                    b.HasKey("UserID", "RaidID");

                    b.ToTable("STDT_RaidAttendees");
                });

            modelBuilder.Entity("STDTBot.Models.RaidInfo", b =>
                {
                    b.Property<long>("RaidID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DateOfRaid")
                        .HasColumnType("datetime");

                    b.HasKey("RaidID");

                    b.ToTable("STDT_Raids");
                });

            modelBuilder.Entity("STDTBot.Models.RankHistory", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DateReset")
                        .HasColumnType("datetime");

                    b.Property<long>("RankID")
                        .HasColumnType("bigint");

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.ToTable("STDT_RankHistory");
                });

            modelBuilder.Entity("STDTBot.Models.RankInfo", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<long>("OfflineRole")
                        .HasColumnType("bigint");

                    b.Property<long>("OnlineRole")
                        .HasColumnType("bigint");

                    b.Property<long>("PointsNeeded")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.ToTable("STDT_Ranks");
                });

            modelBuilder.Entity("STDTBot.Models.Referral", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("ReferralTime")
                        .HasColumnType("datetime");

                    b.Property<long>("ReferredBy")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.ToTable("STDT_Referrals");
                });

            modelBuilder.Entity("STDTBot.Models.SpecialChannel", b =>
                {
                    b.Property<long>("ChannelID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("ChannelType")
                        .HasColumnType("text");

                    b.HasKey("ChannelID");

                    b.ToTable("STDT_Channels");
                });

            modelBuilder.Entity("STDTBot.Models.User", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("CurrentNickname")
                        .HasColumnType("text");

                    b.Property<long>("CurrentPoints")
                        .HasColumnType("bigint");

                    b.Property<long>("CurrentRank")
                        .HasColumnType("bigint");

                    b.Property<string>("Discriminator")
                        .HasColumnType("text");

                    b.Property<long>("HistoricPoints")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsStreaming")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Joined")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("Left")
                        .HasColumnType("datetime");

                    b.Property<string>("UserAvatar")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("STDT_Users");
                });
#pragma warning restore 612, 618
        }
    }
}
