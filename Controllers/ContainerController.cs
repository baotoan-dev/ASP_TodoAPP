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


        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var containers = await _context.TodoContainers
                .Where(c => c.UserId == userId)
                .Include(c => c.TodoItems) // Bao gồm TodoItems
                .ToListAsync();

            // Sử dụng JsonSerializerOptions với ReferenceHandler.Preserve để tránh chu kỳ
            var jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                MaxDepth = 64
            };

            // Serialize các container và TodoItems thành JSON để kiểm tra
            var serializedContainers = JsonSerializer.Serialize(containers, jsonOptions);
            Console.WriteLine(serializedContainers); // Kiểm tra xem đã serialize thành công chưa

            // Trả về kết quả dưới dạng JSON hoặc trong View
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
