
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Enrolments;

public class UpdateEnrolmentStatusRequest
{
    [Required]
    [RegularExpression(
        "^(Pending|Confirmed|Cancelled)$",
        ErrorMessage = "Status must be Pending, Confirmed, or Cancelled.")]
    public string Enrolment_status { get; set; } = "Pending";
}