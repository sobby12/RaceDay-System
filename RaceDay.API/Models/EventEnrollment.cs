
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("EventEnrollments")]
    public class EventEnrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EnrollmentDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Registered";
    }
}
