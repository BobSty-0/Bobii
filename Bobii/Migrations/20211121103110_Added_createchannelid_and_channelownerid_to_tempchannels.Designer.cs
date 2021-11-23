﻿// <auto-generated />
using Bobii.src.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bobii.Migrations
{
    [DbContext(typeof(BobiiEntities))]
    [Migration("20211121103110_Added_createchannelid_and_channelownerid_to_tempchannels")]
    partial class Added_createchannelid_and_channelownerid_to_tempchannels
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

                    b.Property<decimal>("createchannelid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("guildid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("tempchannelname")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("id");

                    b.ToTable("CreateTempChannels");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.filterlink", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("filterlinkactive")
                        .HasColumnType("boolean");

                    b.Property<decimal>("guildid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("id");

                    b.ToTable("FilterLink");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.filterlinklogs", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("channelid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("guildid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("id");

                    b.ToTable("FilterLinkLogs");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.filterlinkoptions", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("bezeichnung")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("linkbody")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("id");

                    b.ToTable("FilterLinkOptions");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.filterlinksguild", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("bezeichnung")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<decimal>("guildid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("id");

                    b.ToTable("FilterLinksGuild");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.filterlinkuserguild", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("guildid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("userid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("id");

                    b.ToTable("FilterLinkUserGuild");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.filterwords", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("filterword")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<decimal>("guildid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("replaceword")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("id");

                    b.ToTable("FilterWords");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.tempchannels", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("channelid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("channelownerid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("createchannelid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("guildid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("id");

                    b.ToTable("TempChannels");
                });
#pragma warning restore 612, 618
        }
    }
}
