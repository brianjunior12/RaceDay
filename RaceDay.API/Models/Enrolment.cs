

namespace RaceDay.API.Models
{

    public class Enrolment 
    {

        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public string Enrolment_status { get; set; } = "Pending";

        public User? Participant { get; set; }

        public Event? Event { get; set; }

        public Category? Category { get; set; }

        public Result? Result { get; set; }

    }
}
