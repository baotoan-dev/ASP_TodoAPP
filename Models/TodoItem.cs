using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace TodoApp.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; } // Thêm thuộc tính UpdatedAt vào đây

        public int ContainerId { get; set; }

        [ForeignKey("ContainerId")]
        [JsonIgnore]
        public TodoContainer? Container { get; set; }
    }
}
