
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        [Required]
        [MaxLength(80)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string CategoryType { get; set; } = string.Empty;

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? CategoryDistanceKm { get; set; }
    }
}

