using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [MaxLength(50, ErrorMessage = "Максимум 50 символов")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;
    }

    public class UpdateCategoryRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;
    }

    public class CategoryResponse()
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
