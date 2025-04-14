// DTOs/LoginDTO.cs
using System.ComponentModel.DataAnnotations;

namespace TodoApp.DTOs
{
    public class LoginDTO
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
