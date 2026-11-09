
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class EnrolmentViewModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        public List<CategoryViewModel> Categories { get; set; }
            = new List<CategoryViewModel>();
    }
}

