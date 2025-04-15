using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using System.Text.Json;

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

        public async Task<IActionResult> Index(DateTime? dueDate)
        {
            Console.WriteLine($"DueDate: {dueDate}"); // Debugging
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var containersQuery = _context.TodoContainers
                .Where(c => c.UserId == userId)
                .Include(c => c.TodoItems)
                .AsQueryable();

            // Nếu có dueDate, lọc theo ngày
            if (dueDate.HasValue)
            {
                containersQuery = containersQuery
                    .Where(c => c.DueDate.HasValue && c.DueDate.Value.Date == dueDate.Value.Date);

                // Truyền lại cho View để hiển thị trong input
                ViewData["DueDate"] = dueDate.Value.ToString("yyyy-MM-dd");
            }

            var containers = await containersQuery.ToListAsync();

            return View(containers);
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

                // check ngày đó đã có container chưa có ròi không cho tạo nữa 
                var existingContainer = await _context.TodoContainers
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.DueDate == container.DueDate);
                if (existingContainer != null)
                {
                    // Nếu đã có container với ngày đó, trả về thông báo lỗi
                    TempData["ErrorMessage"] = "Container with this due date already exists.";
                    return RedirectToAction("Index", "Home");
                }

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

        // Delete method
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var container = await _context.TodoContainers
                                        .Include(c => c.TodoItems)  // Load TodoItems liên quan
                                        .FirstOrDefaultAsync(c => c.Id == id);

            if (container == null || container.UserId != userId)
            {
                return Unauthorized(); // Nếu không tìm thấy hoặc không phải user của container, trả về Unauthorized
            }

            // Xóa các TodoItems liên quan
            _context.TodoItems.RemoveRange(container.TodoItems);

            // Xóa TodoContainer
            _context.TodoContainers.Remove(container);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
