
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Events;

public class UpdateEventRequest
{
    [Required]
    [StringLength(150)]
    public string EventName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Event_Discrip { get; set; }

    [Required]
    public DateTime? EventDate { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "999.99")]
    public decimal Distance { get; set; }

    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;
}