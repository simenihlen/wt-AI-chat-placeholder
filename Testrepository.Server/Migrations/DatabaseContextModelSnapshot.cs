﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;

#nullable disable

namespace Testrepository.Server.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "vector");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ChatMessageEmbeddingEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ChatMessageId")
                        .HasColumnType("integer")
                        .HasColumnName("chatmessageid");

                    b.PrimitiveCollection<double[]>("Embedding")
                        .IsRequired()
                        .HasColumnType("float8[]")
                        .HasColumnName("embedding");

                    b.HasKey("Id");

                    b.HasIndex("ChatMessageId")
                        .IsUnique();

                    b.ToTable("chat_message_embeddings", "testschema");
                });

            modelBuilder.Entity("SessionSummaryEntity", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.PrimitiveCollection<double[]>("embedding")
                        .IsRequired()
                        .HasColumnType("float8[]")
                        .HasColumnName("embedding");

                    b.Property<int>("session_id")
                        .HasColumnType("integer")
                        .HasColumnName("session_id");

                    b.Property<string>("summary_text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("summary_text");

                    b.HasKey("id");

                    b.HasIndex("session_id")
                        .IsUnique();

                    b.ToTable("session_summaries", "testschema");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.BackgroundInfoEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("StoryId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("StoryId");

                    b.ToTable("BackgroundInfos");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.ChatMessageEntity", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<int?>("ChatMessageEmbeddingId")
                        .HasColumnType("integer")
                        .HasColumnName("chatmessageembeddingid");

                    b.Property<bool>("bot")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("bot");

                    b.Property<int>("session_id")
                        .HasColumnType("integer")
                        .HasColumnName("session_id");

                    b.Property<string>("text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("text");

                    b.Property<DateTime>("timestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("user_id")
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("id")
                        .HasName("chat_messages_pkey");

                    b.HasIndex("ChatMessageEmbeddingId");

                    b.HasIndex("session_id");

                    b.HasIndex("user_id");

                    b.ToTable("chat_messages", "testschema");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.ProjectEntity", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string>("sub")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("sub");

                    b.Property<string>("title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.HasIndex("sub");

                    b.ToTable("projects", "testschema");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.SessionEntity", b =>
                {
                    b.Property<int>("session_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("session_id"));

                    b.Property<DateTime>("created_at")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("project_id")
                        .HasColumnType("integer");

                    b.Property<string>("title")
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<string>("user_id")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("session_id")
                        .HasName("sessions_pkey");

                    b.HasIndex("project_id");

                    b.ToTable("sessions", "testschema");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.Story", b =>
                {
                    b.Property<string>("id")
                        .HasColumnType("text");

                    b.Property<int?>("SessionEntitysession_id")
                        .HasColumnType("integer");

                    b.PrimitiveCollection<double[]>("backgrndEmbedding")
                        .IsRequired()
                        .HasColumnType("float8[]")
                        .HasColumnName("background_embedding");

                    b.PrimitiveCollection<double[]>("descrEmbedding")
                        .IsRequired()
                        .HasColumnType("float8[]")
                        .HasColumnName("description_embedding");

                    b.Property<string>("description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<int>("project_id")
                        .HasColumnType("integer");

                    b.Property<string>("title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("id");

                    b.HasIndex("SessionEntitysession_id");

                    b.HasIndex("project_id");

                    b.ToTable("stories", "testschema");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.UserEntity", b =>
                {
                    b.Property<string>("sub")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasColumnName("sub");

                    b.Property<int?>("CurrentSessionsession_id")
                        .HasColumnType("integer");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("sub")
                        .HasName("users_pkey");

                    b.HasIndex("CurrentSessionsession_id");

                    b.ToTable("users", "testschema");
                });

            modelBuilder.Entity("ChatMessageEmbeddingEntity", b =>
                {
                    b.HasOne("Testrepository.Server.Models.Entities.ChatMessageEntity", "ChatMessage")
                        .WithOne()
                        .HasForeignKey("ChatMessageEmbeddingEntity", "ChatMessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ChatMessage");
                });

            modelBuilder.Entity("SessionSummaryEntity", b =>
                {
                    b.HasOne("Testrepository.Server.Models.Entities.SessionEntity", "Session")
                        .WithOne()
                        .HasForeignKey("SessionSummaryEntity", "session_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Session");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.BackgroundInfoEntity", b =>
                {
                    b.HasOne("Testrepository.Server.Models.Entities.Story", "Story")
                        .WithMany("backgroundInfo")
                        .HasForeignKey("StoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Story");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.ChatMessageEntity", b =>
                {
                    b.HasOne("ChatMessageEmbeddingEntity", "ChatMessageEmbedding")
                        .WithMany()
                        .HasForeignKey("ChatMessageEmbeddingId");

                    b.HasOne("Testrepository.Server.Models.Entities.SessionEntity", "session")
                        .WithMany("Messages")
                        .HasForeignKey("session_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_chat_messages_sessions");

                    b.HasOne("Testrepository.Server.Models.Entities.UserEntity", "User")
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("ChatMessageEmbedding");

                    b.Navigation("User");

                    b.Navigation("session");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.ProjectEntity", b =>
                {
                    b.HasOne("Testrepository.Server.Models.Entities.UserEntity", "user")
                        .WithMany("projects")
                        .HasForeignKey("sub")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("user");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.SessionEntity", b =>
                {
                    b.HasOne("Testrepository.Server.Models.Entities.ProjectEntity", "ProjectEntity")
                        .WithMany("sessions")
                        .HasForeignKey("project_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProjectEntity");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.Story", b =>
                {
                    b.HasOne("Testrepository.Server.Models.Entities.SessionEntity", null)
                        .WithMany("Stories")
                        .HasForeignKey("SessionEntitysession_id");

                    b.HasOne("Testrepository.Server.Models.Entities.ProjectEntity", "project")
                        .WithMany("stories")
                        .HasForeignKey("project_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("project");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.UserEntity", b =>
                {
                    b.HasOne("Testrepository.Server.Models.Entities.SessionEntity", "CurrentSession")
                        .WithMany()
                        .HasForeignKey("CurrentSessionsession_id");

                    b.Navigation("CurrentSession");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.ProjectEntity", b =>
                {
                    b.Navigation("sessions");

                    b.Navigation("stories");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.SessionEntity", b =>
                {
                    b.Navigation("Messages");

                    b.Navigation("Stories");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.Story", b =>
                {
                    b.Navigation("backgroundInfo");
                });

            modelBuilder.Entity("Testrepository.Server.Models.Entities.UserEntity", b =>
                {
                    b.Navigation("projects");
                });
#pragma warning restore 612, 618
        }
    }
}
