

namespace RaceDay.API.Models
{
   
    public class Event 
    {
       
        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string Event_Discrip { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public decimal Distance { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public int OrganiserID { get; set; }

        public User? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; }
            = new List<Category>();

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}

