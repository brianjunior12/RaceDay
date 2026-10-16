
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Results;

public class CreateResultRequest
{
    [Range(1, int.MaxValue)]
    public int EnrolmentID { get; set; }

    [Required]
    [RegularExpression(
        @"^([01]\d|2[0-3]):[0-5]\d:[0-5]\d(\.\d{1,7})?$",
        ErrorMessage = "FinishTime must be in HH:mm:ss format.")]
    public string FinishTime { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int FinishPosition { get; set; }
}
