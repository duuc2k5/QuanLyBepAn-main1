using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Data;
using QuanLyBepAn.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBepAn.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.NguoiDung
                .Include(u => u.Quyen)
                .FirstOrDefaultAsync(u => u.TenDangNhap == username && u.MatKhauHash == password);

            if (user != null)
            {
                HttpContext.Session.SetString("User", user.TenDangNhap);
                HttpContext.Session.SetString("MaNguoiDung", user.MaNguoiDung.ToString());
                HttpContext.Session.SetString("Quyen", user.Quyen?.TenQuyen ?? "User"); 
                
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        // --- ĐĂNG KÝ ---
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Load available roles for dropdown
            var roles = await _context.Quyen.ToListAsync();
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, int maQuyen)
        {
            // Reload roles in case of error
            var roles = await _context.Quyen.ToListAsync();
            ViewBag.Roles = roles;

            // Validation
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Tên đăng nhập không được để trống!";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Mật khẩu không được để trống!";
                return View();
            }

            // 1. Kiểm tra Quyền có tồn tại trong bảng Quyen không (Tránh lỗi Foreign Key)
            if (!await _context.Quyen.AnyAsync(q => q.MaQuyen == maQuyen))
            {
                ViewBag.Error = "Quyền được chọn không hợp lệ!";
                return View();
            }

            // 2. Kiểm tra Tên đăng nhập đã tồn tại chưa
            if (await _context.NguoiDung.AnyAsync(u => u.TenDangNhap == username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            // 3. Tạo mới người dùng
            try
            {
                var newUser = new NguoiDung
                {
                    TenDangNhap = username,
                    MatKhauHash = password,
                    MaQuyen = maQuyen
                };

                _context.NguoiDung.Add(newUser);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi đăng ký: {ex.Message}";
                return View();
            }
        }

        // --- ĐĂNG XUẤT ---
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}