

namespace RaceDay.API.Models
{
    
    public class Result 
    {

        public int ResultsID { get; set; }

        public int EnrolmentID { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int FinishPosition { get; set; }

        public Enrolment? Enrolment { get; set; }

    }
}
