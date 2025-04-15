using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace TodoApp.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> View(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var container = await _context.TodoContainers
                .Include(c => c.TodoItems)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (container == null) return NotFound();

            return View(container);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int containerId, string title, string isDone)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var container = await _context.TodoContainers
                .FirstOrDefaultAsync(c => c.Id == containerId && c.UserId == userId);

            if (container == null) return NotFound();

            bool checkDone = false; // Mặc định là chưa hoàn thành

            // Cập nhật trạng thái IsDone
            if (isDone == "true")
            {
                checkDone = true;
            }
            else if (isDone == "false")
            {
                checkDone = false;
            }

            var newTodo = new TodoItem
            {
                Title = title,
                IsDone = checkDone,
                ContainerId = containerId,
                CreatedAt = DateTime.Now
            };

            _context.TodoItems.Add(newTodo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> Edit(int? id, int containerId)
        {
            if (id == null)
            {
                return View("Error");
            }

            var userId = _userManager.GetUserId(User);
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.Container.UserId == userId);

            if (todoItem == null)
            {
                return NotFound();
            }

            var container = await _context.TodoContainers
                .FirstOrDefaultAsync(c => c.Id == containerId && c.UserId == userId);

            if (container == null)
            {
                return NotFound();
            }

            return View(todoItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(TodoItem todoItem, int containerId)
        {
            var userId = _userManager.GetUserId(User);
            var container = await _context.TodoContainers
                .FirstOrDefaultAsync(c => c.Id == containerId && c.UserId == userId);

            if (container == null)
            {
                return NotFound();
            }

            if (todoItem.Id == 0) // Thêm mới
            {
                todoItem.ContainerId = containerId;
                todoItem.CreatedAt = DateTime.Now;
                _context.TodoItems.Add(todoItem);
            }
            else // Cập nhật
            {
                var existingTodo = await _context.TodoItems
                    .FirstOrDefaultAsync(t => t.Id == todoItem.Id && t.ContainerId == containerId);

                if (existingTodo == null)
                {
                    return NotFound();
                }

                existingTodo.Title = todoItem.Title;
                existingTodo.IsDone = todoItem.IsDone; // IsDone đã là bool, không cần chuyển đổi
                existingTodo.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("View", "Todo", new { id = containerId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.Container.UserId == userId);

            if (todoItem == null) return NotFound();

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
