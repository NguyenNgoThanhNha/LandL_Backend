using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("BlogRating")]
    public class BlogRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogRatingId { get; set; }

        [Required]
        public int Rating { get; set; }

        public int TotalRating { get; set; }

        [ForeignKey("BlogRating")]
        public int BlogId { get; set; }
        public virtual Blog BlogRate { get; set; }

        [ForeignKey("UserRating")]
        public int UserId { get; set; }
        public virtual User UserRate { get; set; }
    }
}
