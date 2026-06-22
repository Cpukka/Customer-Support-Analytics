using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerSupportAPI.Models
{
    [Table("tickets", Schema = "support")]
    public class Ticket
    {
        [Key]
        [Column("ticket_id")]
        public int TicketID { get; set; }

        [Required]
        [Column("customer_id")]
        public int CustomerID { get; set; }

        [Required]
        [Column("subject")]
        [StringLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Column("body")]
        public string Body { get; set; } = string.Empty;

        [Required]
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "Open";

        [Required]
        [Column("priority")]
        [StringLength(10)]
        public string Priority { get; set; } = "Medium";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }
        public virtual AIAnalysis? AIAnalysis { get; set; }
    }
}