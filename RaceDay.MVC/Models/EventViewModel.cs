
namespace RaceDay.MVC.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        public int OrganizerId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public double DistanceKm { get; set; }

        public string EventType { get; set; } = string.Empty;

        public decimal EntryFee { get; set; }

        public int MaxParticipants { get; set; }
    }
}

