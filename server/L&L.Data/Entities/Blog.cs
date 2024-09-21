using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("Blog")]
    public class Blog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogId { get; set; }

        public string? title { get; set; } = string.Empty;

        public string? content { get; set; } = string.Empty;

        public string? description { get; set; } = string.Empty;

        [Required]
        public string thumbnail { get; set; } = string.Empty;

        [Required]
        public string status { get; set; } = string.Empty;

        [ForeignKey("UserBlog")]
        public int UserId { get; set; }
        public virtual User UserBlog { get; set; }
    }
}
