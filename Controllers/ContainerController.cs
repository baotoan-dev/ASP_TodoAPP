using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Authorize]
    public class ContainerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContainerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var containers = await _context.TodoContainers
            .Where(c => c.UserId == userId)
            .Include(c => c.TodoItems) // Including TodoItems if needed for future reference
            .ToListAsync();

        // Debugging: Kiểm tra xem có dữ liệu không
        Console.WriteLine($"Found {containers.Count} containers");

        return RedirectToAction("Index", "Home", new { containers = containers });
    }


      public async Task<IActionResult> Save(TodoContainer container)
        {
            Console.WriteLine("Save method called");
            Console.WriteLine($"Container: {System.Text.Json.JsonSerializer.Serialize(container)}");
            
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu không có userId, điều hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }

            if (container.Id == 0) // Tạo mới
            {
                container.UserId = userId;
                container.CreatedAt = DateTime.Now;
                _context.TodoContainers.Add(container);
            }
            else // Chỉnh sửa
            {
                var existing = await _context.TodoContainers.FindAsync(container.Id);
                if (existing == null || existing.UserId != userId)
                {
                    return Unauthorized(); // Nếu không tìm thấy hoặc không phải user của container, trả về Unauthorized
                }

                existing.Title = container.Title;
                existing.ThemeColor = container.ThemeColor;
                existing.DueDate = container.DueDate;
                existing.UpdatedAt = DateTime.Now;
            }

            // Kiểm tra lưu vào cơ sở dữ liệu
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi tại đây (ví dụ: lưu vào file log hoặc database)
                return View("Error");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
