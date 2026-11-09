
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Results")]
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        public int EnrollmentId { get; set; }

        public int? FinishPosition { get; set; }

        public int? FinishTimeSeconds { get; set; }

        [Required]
        [MaxLength(20)]
        public string ResultStatus { get; set; } = "Official";
    }
}

