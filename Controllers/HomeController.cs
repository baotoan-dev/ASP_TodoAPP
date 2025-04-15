using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // ✅ Quan trọng
using TodoApp.Models;
using TodoApp.Data;

namespace TodoApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;

        return View(containers);
    }

}
