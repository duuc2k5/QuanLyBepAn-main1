using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Data;
using QuanLyBepAn.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;

// Class để receive admin data từ AJAX
public class AdminDataRequest
{
    public string? TenDangNhap { get; set; }
    public string? MatKhau { get; set; }
    public int MaQuyen { get; set; }
}

namespace QuanLyBepAn.Controllers
{
    public class NguoiDungController : BaseController
    {
        private readonly IConfiguration _configuration;

        public NguoiDungController(ApplicationDbContext context, IConfiguration configuration) 
            : base(context) 
        { 
            _configuration = configuration;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Quyen") == "Admin";

        // --- INDEX: Sửa lỗi 404 ---
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            
            // Dùng .Include để lấy thông tin bảng Quyen kèm theo
            var dsNguoiDung = await _context.NguoiDung.Include(n => n.Quyen).ToListAsync();
            return View(dsNguoiDung);
        }

        // --- DETAILS ---
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            if (id == null) return NotFound();

            var nguoiDung = await _context.NguoiDung.Include(n => n.Quyen)
                .FirstOrDefaultAsync(m => m.MaNguoiDung == id);
            
            return nguoiDung == null ? NotFound() : View(nguoiDung);
        }

        // --- CREATE (GET): Nạp dữ liệu cho Dropdown ---
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            
            ViewBag.MaQuyen = new SelectList(_context.Quyen, "MaQuyen", "TenQuyen");
            return View();
        }

        // --- CREATE (POST): Xử lý lưu dữ liệu ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDangNhap,MatKhauHash,MaQuyen")] NguoiDung nguoiDung)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");

            ModelState.Remove("MaNguoiDung"); 

            // Nếu không phải Admin, tạo bình thường
            if (ModelState.IsValid)
            {
                try 
                {
                    _context.Add(nguoiDung);
                    await _context.SaveChangesAsync();
                    var quyenName = await _context.Quyen.Where(q => q.MaQuyen == nguoiDung.MaQuyen).Select(q => q.TenQuyen).FirstOrDefaultAsync();
                    GhiNhatKy($"Đã tạo người dùng: {nguoiDung.TenDangNhap} (Quyền: {quyenName})");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Lỗi lưu vào CSDL: " + ex.Message);
                }
            }
            
            ViewBag.MaQuyen = new SelectList(_context.Quyen, "MaQuyen", "TenQuyen", nguoiDung.MaQuyen);
            return View(nguoiDung);
        }

        // --- CONFIRM ADMIN KEY (GET): Hiển thị form xác nhận key admin ---
        public IActionResult ConfirmAdminKey()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            
            var tenDangNhap = HttpContext.Session.GetString("TempTenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction(nameof(Create));
            
            ViewBag.TenDangNhap = tenDangNhap;
            return View();
        }

        // --- STORE ADMIN DATA (AJAX): Lưu dữ liệu admin tạm vào session ---
        [HttpPost]
        public IActionResult StoreAdminData([FromBody] AdminDataRequest data)
        {
            if (!IsAdmin()) return Unauthorized();

            if (string.IsNullOrEmpty(data?.TenDangNhap) || string.IsNullOrEmpty(data?.MatKhau))
                return BadRequest();

            HttpContext.Session.SetString("TempTenDangNhap", data.TenDangNhap);
            HttpContext.Session.SetString("TempMatKhauHash", data.MatKhau);
            HttpContext.Session.SetInt32("TempMaQuyen", data.MaQuyen);

            return Ok();
        }

        // --- CONFIRM ADMIN KEY (POST): Xử lý xác nhận key admin ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAdminKey(string AdminKey)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            
            var tenDangNhap = HttpContext.Session.GetString("TempTenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction(nameof(Create));

            // Kiểm tra key
            var configKey = _configuration["AdminSecurityKey"];
            if (string.IsNullOrEmpty(AdminKey) || AdminKey != configKey)
            {
                // Clear temp session and redirect back to Create with an error message
                HttpContext.Session.Remove("TempTenDangNhap");
                HttpContext.Session.Remove("TempMatKhauHash");
                HttpContext.Session.Remove("TempMaQuyen");
                TempData["AdminKeyError"] = "Key admin không chính xác!";
                return RedirectToAction(nameof(Create));
            }

            // Key đúng, tạo người dùng
            try
            {
                var matKhauHash = HttpContext.Session.GetString("TempMatKhauHash") ?? "";
                var maQuyen = HttpContext.Session.GetInt32("TempMaQuyen") ?? 0;

                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = tenDangNhap,
                    MatKhauHash = matKhauHash,
                    MaQuyen = maQuyen
                };

                _context.Add(nguoiDung);
                await _context.SaveChangesAsync();
                
                var quyenName = await _context.Quyen.Where(q => q.MaQuyen == maQuyen).Select(q => q.TenQuyen).FirstOrDefaultAsync();
                GhiNhatKy($"Đã tạo tài khoản Admin: {tenDangNhap} (Quyền: {quyenName})");

                // Xóa dữ liệu tạm khỏi session
                HttpContext.Session.Remove("TempTenDangNhap");
                HttpContext.Session.Remove("TempMatKhauHash");
                HttpContext.Session.Remove("TempMaQuyen");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.TenDangNhap = tenDangNhap;
                ViewBag.Error = $"Lỗi tạo tài khoản: {ex.Message}";
                return View();
            }
        }

        // --- EDIT (GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            if (id == null) return NotFound();

            var nguoiDung = await _context.NguoiDung.FindAsync(id);
            if (nguoiDung == null) return NotFound();
            
            ViewBag.MaQuyen = new SelectList(_context.Quyen, "MaQuyen", "TenQuyen", nguoiDung.MaQuyen);
            return View(nguoiDung);
        }

        // --- EDIT (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNguoiDung,TenDangNhap,MatKhauHash,MaQuyen")] NguoiDung nguoiDung)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            if (id != nguoiDung.MaNguoiDung) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu cũ để so sánh và ghi log chi tiết (AsNoTracking để tránh lỗi tracking)
                    var oldData = await _context.NguoiDung.AsNoTracking().FirstOrDefaultAsync(n => n.MaNguoiDung == id);
                    
                    if (oldData != null)
                    {
                        // Kiểm tra nếu tên đăng nhập thay đổi
                        if (oldData.TenDangNhap != nguoiDung.TenDangNhap)
                        {
                            GhiNhatKy($"Đổi tên người dùng: '{oldData.TenDangNhap}' → '{nguoiDung.TenDangNhap}'");
                        }

                        // Kiểm tra nếu mật khẩu thay đổi
                        if (oldData.MatKhauHash != nguoiDung.MatKhauHash)
                        {
                            GhiNhatKy($"Đổi mật khẩu cho người dùng: {nguoiDung.TenDangNhap}");
                        }

                        // Kiểm tra nếu quyền thay đổi
                        if (oldData.MaQuyen != nguoiDung.MaQuyen)
                        {
                            var oldQuyen = await _context.Quyen.AsNoTracking().FirstOrDefaultAsync(q => q.MaQuyen == oldData.MaQuyen);
                            var newQuyen = await _context.Quyen.AsNoTracking().FirstOrDefaultAsync(q => q.MaQuyen == nguoiDung.MaQuyen);
                            GhiNhatKy($"Thay đổi quyền cho {nguoiDung.TenDangNhap}: '{oldQuyen?.TenQuyen}' → '{newQuyen?.TenQuyen}'");
                        }
                    }
                    
                    _context.Update(nguoiDung);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi lưu dữ liệu: {ex.Message}");
                }
            }
            else
            {
                // Log validation errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
            }
            
            ViewBag.MaQuyen = new SelectList(_context.Quyen, "MaQuyen", "TenQuyen", nguoiDung.MaQuyen);
            return View(nguoiDung);
        }

        // --- DELETE (GET): Display confirmation page ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            if (id == null) return NotFound();

            var nguoiDung = await _context.NguoiDung.Include(n => n.Quyen)
                .FirstOrDefaultAsync(m => m.MaNguoiDung == id);
            
            return nguoiDung == null ? NotFound() : View(nguoiDung);
        }

        // --- DELETE (POST): Confirm deletion ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            var nguoiDung = await _context.NguoiDung.FindAsync(id);
            if (nguoiDung != null)
            {
                _context.NguoiDung.Remove(nguoiDung);
                await _context.SaveChangesAsync();
                GhiNhatKy("Đã xóa người dùng: " + nguoiDung.TenDangNhap);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}