﻿// <auto-generated />
using Bobii.src.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bobii.Migrations.BobiiLngCodesMigrations
{
    [DbContext(typeof(BobiiLngCodes))]
    [Migration("20231205164820_ru_hinzugefuegt")]
    partial class ru_hinzugefuegt
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.caption", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("de")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("en")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("msgid")
                        .HasMaxLength(4)
                        .HasColumnType("character varying(4)");

                    b.Property<string>("ru")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("id");

                    b.ToTable("Captions");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.commands", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("command")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("de")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("en")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ru")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("id");

                    b.ToTable("Commands");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.content", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("de")
                        .HasMaxLength(400)
                        .HasColumnType("character varying(400)");

                    b.Property<string>("en")
                        .HasMaxLength(400)
                        .HasColumnType("character varying(400)");

                    b.Property<string>("msgid")
                        .HasMaxLength(4)
                        .HasColumnType("character varying(4)");

                    b.Property<string>("ru")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("id");

                    b.ToTable("Contents");
                });

            modelBuilder.Entity("Bobii.src.EntityFramework.Entities.language", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("guildid")
                        .HasMaxLength(18)
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("langugeshort")
                        .HasMaxLength(2)
                        .HasColumnType("character varying(2)");

                    b.HasKey("id");

                    b.ToTable("Languages");
                });
#pragma warning restore 612, 618
        }
    }
}
