﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using YSpotify;

#nullable disable

namespace YSpotify.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240229082524_SeedGroup")]
    partial class SeedGroup
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("YSpotify.Group", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name")
                        .HasAnnotation("Relational:JsonPropertyName", "name");

                    b.Property<long>("LeaderId")
                        .HasColumnType("bigint")
                        .HasColumnName("leader_id");

                    b.HasKey("Name");

                    b.HasIndex("LeaderId");

                    b.ToTable("groups");

                    b.HasData(
                        new
                        {
                            Name = "TestGroup",
                            LeaderId = 1L
                        });
                });

            modelBuilder.Entity("YSpotify.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("GroupName")
                        .HasColumnType("text")
                        .HasColumnName("group_name")
                        .HasAnnotation("Relational:JsonPropertyName", "group_name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password")
                        .HasAnnotation("Relational:JsonPropertyName", "password");

                    b.Property<string>("SpotifyAccessToken")
                        .HasColumnType("text")
                        .HasColumnName("spotify_access_token");

                    b.Property<long?>("SpotifyAccessTokenExpiration")
                        .HasColumnType("bigint")
                        .HasColumnName("spotify_access_token_expiration");

                    b.Property<string>("SpotifyRefreshToken")
                        .HasColumnType("text")
                        .HasColumnName("spotify_refresh_token");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username")
                        .HasAnnotation("Relational:JsonPropertyName", "username");

                    b.HasKey("Id");

                    b.HasIndex("GroupName");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("users");

                    b.HasAnnotation("Relational:JsonPropertyName", "leader");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            GroupName = "TestGroup",
                            Password = "Dk8ZVuZjmsgVtJDfLv74gA3Rc4+D63N4lGH6JvauMvA=",
                            Username = "TestUser"
                        });
                });

            modelBuilder.Entity("YSpotify.Group", b =>
                {
                    b.HasOne("YSpotify.User", "Leader")
                        .WithMany()
                        .HasForeignKey("LeaderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Leader");
                });

            modelBuilder.Entity("YSpotify.User", b =>
                {
                    b.HasOne("YSpotify.Group", "Group")
                        .WithMany("Members")
                        .HasForeignKey("GroupName");

                    b.Navigation("Group");
                });

            modelBuilder.Entity("YSpotify.Group", b =>
                {
                    b.Navigation("Members");
                });
#pragma warning restore 612, 618
        }
    }
}
