﻿// <auto-generated />
using System;
using Bobii.src.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bobii.Migrations
{
    [DbContext(typeof(BobiiEntities))]
    [Migration("20231003114647_autodelet_inconfig_hinzugefuegt")]
    partial class autodelet_inconfig_hinzugefuegt
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.createtempchannels", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("autodelete")
                        .HasColumnType("integer");

                    b.Property<int?>("channelsize")
                        .HasColumnType("integer");

                    b.Property<decimal>("createchannelid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("delay")
                        .HasColumnType("integer");

                    b.Property<decimal>("guildid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("tempchannelname")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("id");

                    b.ToTable("CreateTempChannels");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.tempchannels", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("channelid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("channelownerid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("count")
                        .HasColumnType("integer");

                    b.Property<decimal?>("createchannelid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime?>("deletedate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("guildid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long>("unixtimestamp")
                        .HasColumnType("bigint");

                    b.HasKey("id");

                    b.ToTable("TempChannels");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.tempchanneluserconfig", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("autodelete")
                        .HasColumnType("integer");

                    b.Property<int?>("channelsize")
                        .HasColumnType("integer");

                    b.Property<decimal>("createchannelid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("guildid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("tempchannelname")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<decimal>("userid")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("id");

                    b.ToTable("TempChannelUserConfigs");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.tempcommands", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("commandname")
                        .HasColumnType("text");

                    b.Property<decimal>("createchannelid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("enabled")
                        .HasColumnType("boolean");

                    b.Property<decimal>("guildguid")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("id");

                    b.ToTable("Commands");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.usedfunctions", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("affecteduserid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("channelid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("doneat")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("function")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<decimal>("guildid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("isuser")
                        .HasColumnType("boolean");

                    b.Property<decimal>("userid")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("id");

                    b.ToTable("UsedFunctions");
                });
#pragma warning restore 612, 618
        }
    }
}
