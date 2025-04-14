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

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var containers = await _context.TodoContainers
            .Where(c => c.UserId == userId)
            .Include(c => c.TodoItems)
            .ToListAsync();

        Console.WriteLine($"Containers: {System.Text.Json.JsonSerializer.Serialize(containers)}"); // Debugging
        Console.WriteLine($"Found {containers.Count} containers");

        return View(containers); // Đảm bảo View Index.cshtml dùng @model List<TodoContainer>
    }
}
