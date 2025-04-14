using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TodoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int containerId, string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                _context.TodoItems.Add(new TodoItem
                {
                    Title = title,
                    ContainerId = containerId
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Container", new { id = containerId });
        }

        public async Task<IActionResult> Toggle(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item != null)
            {
                item.IsDone = !item.IsDone;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Container", new { id = item.ContainerId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item != null)
            {
                var containerId = item.ContainerId;
                _context.TodoItems.Remove(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Container", new { id = containerId });
            }
            return NotFound();
        }
    }
}
