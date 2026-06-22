using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerSupportAPI.Models
{
    [Table("ai_analysis", Schema = "support")]
    public class AIAnalysis
    {
        [Key]
        [Column("analysis_id")]
        public int AnalysisID { get; set; }

        [Required]
        [Column("ticket_id")]
        public int TicketID { get; set; }

        [Required]
        [Column("sentiment_score")]
        [Range(-1.0, 1.0)]
        public decimal SentimentScore { get; set; }

        [Required]
        [Column("sentiment_label")]
        [StringLength(20)]
        public string SentimentLabel { get; set; } = string.Empty;

        [Required]
        [Column("category")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Column("keywords")]
        [StringLength(500)]
        public string? Keywords { get; set; }

        [Column("processed_at")]
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("TicketID")]
        public virtual Ticket? Ticket { get; set; }
    }
}