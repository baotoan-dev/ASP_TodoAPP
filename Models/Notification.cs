using TodoApp.Models;

public class NotificationModel
{
    public int Id { get; set; } // Thêm Id làm khóa chính
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // success, error, info, warning
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? RedirectUrl { get; set; }

    public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}
