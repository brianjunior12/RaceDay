

namespace RaceDay.API.Models
{
   
    public class Category 
    {

        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string CategoryType { get; set; } = string.Empty;

        public int EventID { get; set; }

        public Event? Event { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();

    }
}
