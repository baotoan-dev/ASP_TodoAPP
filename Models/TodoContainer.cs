using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace TodoApp.Models
{
    public class TodoContainer
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string ThemeColor { get; set; } = "#ffffff";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

    [JsonIgnore]
        public List<TodoItem> TodoItems { get; set; } = new();
    }
}
