using Microsoft.EntityFrameworkCore;
using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Persistence.Internal.GeneratedArtifacts;

public partial class DatabaseContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }


    public virtual DbSet<ChatMessageEntity> ChatMessages { get; set; }
    public virtual DbSet<SessionEntity> Sessions { get; set; }
    public virtual DbSet<UserEntity> UserEntities { get; set; }
    public DbSet<ChatMessageEmbeddingEntity> ChatMessageEmbeddings { get; set; }
    public DbSet<SessionSummaryEntity> SessionsSummaries { get; set; }
    public DbSet<ProjectStories> ProjectStories { get; set; }

    public virtual DbSet<ProjectEntity> ProjectEntities { get; set; }

    public DbSet<Story> Stories { get; set; }
    
    public DbSet<BackgroundInfoEntity> BackgroundInfos { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration["DatabaseCredentials:ConnectionString"];
            optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());  // ✅ Ensure Npgsql and vector extension is used
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.Entity<ChatMessageEntity>(entity =>
        {
            entity.HasKey(e => e.id).HasName("chat_messages_pkey");
            entity.ToTable("chat_messages", "testschema");

            entity.Property(e => e.id).HasColumnName("id");
            entity.Property(e => e.user_id).HasColumnName("user_id");
            entity.Property(e => e.bot)
                .HasColumnName("bot")
                .HasDefaultValue(false);
            entity.Property(e => e.session_id).HasColumnName("session_id");
            entity.Property(e => e.text).HasColumnName("text");
            entity.Property(e => e.timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.session)
                .WithMany(p => p.Messages)
                .HasForeignKey(d => d.session_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_chat_messages_sessions");
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChatMessageEmbedding)
                .WithOne(e => e.ChatMessage)
                .HasForeignKey<ChatMessageEmbeddingEntity>(e => e.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ChatMessageEmbedding)
                .WithOne(e => e.ChatMessage)
                .HasForeignKey<ChatMessageEmbeddingEntity>(e => e.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SessionEntity>(entity =>
        {
            entity.HasKey(e => e.session_id).HasName("sessions_pkey");
            entity.ToTable("sessions", "testschema");

            entity.Property(e => e.session_id).HasColumnName("id");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.title) 
                .HasColumnName("title")
                .HasColumnType("text");

            entity.HasOne(s => s.ProjectEntity)
                .WithMany(p => p.sessions)
                .HasForeignKey(p => p.session_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.sub).HasName("users_pkey");
            entity.ToTable("users", "testschema");

            // Map the "sub" property to the "sub" column
            entity.Property(e => e.sub)
                .HasColumnName("sub")
                .IsRequired();

            
            entity.HasOne(e => e.DefaultProject)
                .WithMany()
                .HasForeignKey(e => e.defaultProjectId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.CurrentProject)
                .WithMany()
                .HasForeignKey(e => e.currentProjectId)
                .OnDelete(DeleteBehavior.Restrict); // or .SetNull / .NoAction based on your needs



            // Note: Ensure that the related foreign key property in SessionEntity (currently "user_id")
            // is updated to be a string to match the type of "sub".
            entity.HasMany(u => u.projects)
                .WithOne(p => p.user)
                .HasForeignKey(s => s.sub)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<ChatMessageEmbeddingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("chat_message_embeddings", "testschema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChatMessageId).HasColumnName("chatmessageid"); // Fix PascalCase issue

            entity.Property(e => e.Embedding)
                .HasColumnType("float8[]"); // Correct PostgreSQL type for embeddings

            entity.HasOne(e => e.ChatMessage)
                .WithOne()
                .HasForeignKey<ChatMessageEmbeddingEntity>(e => e.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<SessionSummaryEntity>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.ToTable("session_summaries", "testschema");

            entity.Property(e => e.summary_text).HasColumnName("summary_text");
            entity.Property(e => e.embedding).HasColumnType("float8[]");
            entity.Property(e => e.last_character_count).HasColumnName("last_character_count").HasColumnType("integer");

            entity.HasOne(e => e.Session)
                .WithOne()
                .HasForeignKey<SessionSummaryEntity>(e => e.session_id)
                .OnDelete(DeleteBehavior.Cascade);  // If session is deleted, summary is also deleted
        });

        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.ToTable("stories", "testschema");

            entity.Property(e => e.title).HasColumnName("title");
           // entity.Property(e => e.description).HasColumnName("description");
            entity.Property(e => e.storySummary).HasColumnName("storySummary");
            entity.Property(e => e.descrEmbedding).HasColumnName("description_embedding").HasColumnType("float8[]");
            entity.Property(e => e.backgrndEmbedding).HasColumnName("background_embedding").HasColumnType("float8[]");

        
                
                entity.HasMany(s => s.backgroundInfo)
                .WithOne(b => b.Story)
                .HasForeignKey(b => b.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProjectEntity>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.ToTable("projects", "testschema");

            entity.Property(s => s.id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(e => e.sub)
                  .HasColumnName("sub")
                  .IsRequired();
            

            entity.HasOne(s => s.user)
                .WithMany(u => u.projects)
                .HasForeignKey(s => s.sub)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.sessions)
                .WithOne(sess => sess.ProjectEntity)
                .HasForeignKey(sess => sess.project_id)
                .OnDelete(DeleteBehavior.Cascade);
            

        });
        
        modelBuilder.Entity<ProjectStories>(entity =>
        {
            entity.HasKey(ps => ps.Id);
            entity.ToTable("project_stories", "testschema");

            entity.HasIndex(ps => new { ps.ProjectId, ps.StoryId }).IsUnique();

            entity.HasOne(ps => ps.Project)
                .WithMany(p => p.projectStories)
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ps => ps.Story)
                .WithMany(s => s.projectStories)
                .HasForeignKey(ps => ps.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}