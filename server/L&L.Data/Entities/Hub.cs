using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("Hub")]
    public class Hub
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HubId { get; set; }

        [ForeignKey("Sender")]
        public int SenderId { get; set; }
        public virtual User Sender { get; set; }

        [ForeignKey("Recipient")]
        public int RecipientId { get; set; }
        public virtual User Recipient { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
