// Controllers/AccountController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;
using TodoApp.DTOs;
using Microsoft.AspNetCore.Authorization;  // Đảm bảo dòng này có mặt

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        Console.WriteLine("Register method called");
        Console.WriteLine($"DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email, FullName = dto.FullName };
            var result = await _userManager.CreateAsync(user, dto.Password);
            Console.WriteLine($"Result: {System.Text.Json.JsonSerializer.Serialize(result)}");

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "Registration successful!";  // Lưu thông báo thành công
                return RedirectToAction("Index", "Home");
            }

            // Thêm lỗi vào ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Nếu có lỗi, trả lại form và hiển thị lỗi
        ViewData["Title"] = "Register";
        return View(dto);
    }


    // Controllers/AccountController.cs
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        ViewData["Title"] = "Login";
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Login";
            return View(dto);
        }

        var result = await _signInManager.PasswordSignInAsync(dto.UserName, dto.Password, false, false);
        Console.WriteLine($"Login result: {result}");

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        ViewData["Title"] = "Login";
        return View("Login"); // Trả về view Login với thông tin đăng nhập không hợp lệ
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        Console.WriteLine("Logout method called");
        await _signInManager.SignOutAsync();
        ViewData["Title"] = "Login";
        TempData["SuccessMessage"] = "Logout successful!";  // Lưu thông báo thành công
        return View("Login"); // Redirect to login page after logout
    }

    // Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        ViewData["Title"] = "Register";
        return View(new RegisterDTO());
    }
}
