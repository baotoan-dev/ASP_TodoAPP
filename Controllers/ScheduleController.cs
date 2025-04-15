using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // ✅ Quan trọng
using TodoApp.Models;
using TodoApp.Data;

namespace TodoApp.Controllers;

[Authorize]
public class ScheduleController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ScheduleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        var today = DateTime.Today;
        from ??= today.AddDays(-(int)today.DayOfWeek); // Bắt đầu từ Chủ nhật
        to ??= from.Value.AddDays(6); // Kết thúc sau 6 ngày = 1 tuần

        var containers = await _context.TodoContainers
            .Where(c => c.UserId == userId && c.DueDate >= from && c.DueDate <= to)
            .Include(c => c.TodoItems)
            .ToListAsync();

        ViewBag.From = from.Value;
        ViewBag.To = to.Value;

        return View(containers);
    }

}