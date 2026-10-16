using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Categories;

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CategoryType { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int EventID { get; set; }
}


