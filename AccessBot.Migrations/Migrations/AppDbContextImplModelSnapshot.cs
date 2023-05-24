﻿// <auto-generated />
using System;
using AccessBot.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccessBot.Migrations.Migrations
{
    [DbContext(typeof(AppDbContextImpl))]
    partial class AppDbContextImplModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AccessBot.Application.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("Balance")
                        .HasColumnType("integer");

                    b.Property<long?>("InviterId")
                        .HasColumnType("bigint");

                    b.Property<Instant>("PaidUntil")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("InviterId");

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("AccessBot.Application.Models.User", b =>
                {
                    b.HasOne("AccessBot.Application.Models.User", "Inviter")
                        .WithMany("Referrals")
                        .HasForeignKey("InviterId");

                    b.Navigation("Inviter");
                });

            modelBuilder.Entity("AccessBot.Application.Models.User", b =>
                {
                    b.Navigation("Referrals");
                });
#pragma warning restore 612, 618
        }
    }
}
