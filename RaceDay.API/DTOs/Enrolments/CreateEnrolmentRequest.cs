
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Enrolments;

public class CreateEnrolmentRequest
{
    [Range(1, int.MaxValue)]
    public int EventID { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryID { get; set; }
}