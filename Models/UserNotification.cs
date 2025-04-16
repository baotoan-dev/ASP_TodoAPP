
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using TodoApp.Models;
public class UserNotification
{
    public int Id { get; set; }

    // Liên kết với User
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    // Liên kết với Notification
    public int NotificationId { get; set; }
    public NotificationModel Notification { get; set; }

    // Trạng thái
    public bool IsRead { get; set; } = false;
    public DateTime ReceivedAt { get; set; } = DateTime.Now;
}
