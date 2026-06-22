using Microsoft.EntityFrameworkCore;
using CustomerSupportAPI.Models;


namespace CustomerSupportAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<AIAnalysis> AIAnalyses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set schema
            modelBuilder.HasDefaultSchema("support");

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customers");
                entity.HasKey(e => e.CustomerID);
                entity.Property(e => e.CustomerID).HasColumnName("customer_id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Company).HasColumnName("company").HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Ticket configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("tickets");
                entity.HasKey(e => e.TicketID);
                entity.Property(e => e.TicketID).HasColumnName("ticket_id");
                entity.Property(e => e.CustomerID).HasColumnName("customer_id");
                entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(255);
                entity.Property(e => e.Body).HasColumnName("body");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20)
                    .HasDefaultValue("Open");
                entity.Property(e => e.Priority).HasColumnName("priority").HasMaxLength(10)
                    .HasDefaultValue("Medium");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Tickets)
                    .HasForeignKey(e => e.CustomerID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CustomerID);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // AIAnalysis configuration
            modelBuilder.Entity<AIAnalysis>(entity =>
            {
                entity.ToTable("ai_analysis");
                entity.HasKey(e => e.AnalysisID);
                entity.Property(e => e.AnalysisID).HasColumnName("analysis_id");
                entity.Property(e => e.TicketID).HasColumnName("ticket_id");
                entity.Property(e => e.SentimentScore).HasColumnName("sentiment_score")
                    .HasPrecision(5, 4);
                entity.Property(e => e.SentimentLabel).HasColumnName("sentiment_label")
                    .HasMaxLength(20);
                entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
                entity.Property(e => e.Keywords).HasColumnName("keywords").HasMaxLength(500);
                entity.Property(e => e.ProcessedAt).HasColumnName("processed_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Ticket)
                    .WithOne(t => t.AIAnalysis)
                    .HasForeignKey<AIAnalysis>(e => e.TicketID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TicketID).IsUnique();
                entity.HasIndex(e => e.SentimentLabel);
                entity.HasIndex(e => e.Category);
            });
        }
    }
}