using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Data;
using QuanLyBepAn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace QuanLyBepAn.Controllers
{
    public class KhoController : BaseController
    {
        public KhoController(ApplicationDbContext context) : base(context) { }

        // Allow both "Thủ kho" and "Admin" roles to manage inventory
        private bool IsThuKho()
        {
            var q = HttpContext.Session.GetString("Quyen");
            return q == "Thủ kho" || q == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsThuKho()) return RedirectToAction("AccessDenied", "Home");
            // Include NguyenLieu so related fields (Calo, Dam, Cali) are available in the view
            return View(await _context.Kho.Include(k => k.NguyenLieu).ToListAsync());
        }

        public IActionResult Create()
        {
            if (!IsThuKho()) return RedirectToAction("AccessDenied", "Home");
            ViewBag.MaNguyenLieu = new SelectList(_context.NguyenLieu, "MaNguyenLieu", "TenNguyenLieu");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNguyenLieu,SoLuongTon")] Kho kho, string NewTenNguyenLieu, double? NewCalo, double? NewDam, double? NewCali)
        {
            if (!IsThuKho()) return RedirectToAction("AccessDenied", "Home");

            // If a new ingredient name is provided, create it and set MaNguyenLieu
            if (!string.IsNullOrWhiteSpace(NewTenNguyenLieu))
            {
                var ng = new NguyenLieu
                {
                    TenNguyenLieu = NewTenNguyenLieu,
                    Calo = NewCalo ?? 0,
                    Dam = NewDam ?? 0,
                    Cali = NewCali ?? 0
                };
                try
                {
                    _context.NguyenLieu.Add(ng);
                    await _context.SaveChangesAsync();
                    // assign created id to kho
                    kho.MaNguyenLieu = ng.MaNguyenLieu;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi lưu nguyên liệu mới: {ex.Message}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(kho);
                    await _context.SaveChangesAsync();
                    GhiNhatKy("Đã cập nhật kho: " + kho.MaKho);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi lưu dữ liệu kho: {ex.Message}");
                }
            }

            ViewBag.MaNguyenLieu = new SelectList(_context.NguyenLieu, "MaNguyenLieu", "TenNguyenLieu", kho.MaNguyenLieu);
            return View(kho);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!IsThuKho()) return RedirectToAction("AccessDenied", "Home");
            if (id == null) return NotFound();

            var kho = await _context.Kho.Include(k => k.NguyenLieu)
                .FirstOrDefaultAsync(k => k.MaKho == id);
            
            return kho == null ? NotFound() : View(kho);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsThuKho()) return RedirectToAction("AccessDenied", "Home");
            if (id == null) return NotFound();

            var kho = await _context.Kho.FindAsync(id);
            if (kho == null) return NotFound();
            
            ViewBag.MaNguyenLieu = new SelectList(_context.NguyenLieu, "MaNguyenLieu", "TenNguyenLieu", kho.MaNguyenLieu);
            return View(kho);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKho,MaNguyenLieu,SoLuongTon")] Kho kho)
        {
            if (!IsThuKho()) return RedirectToAction("AccessDenied", "Home");
            if (id != kho.MaKho) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch with AsNoTracking first to avoid tracking conflicts
                    var existing = await _context.Kho.AsNoTracking().FirstOrDefaultAsync(k => k.MaKho == id);
                    if (existing == null) return NotFound();
                    
                    _context.Update(kho);
                    await _context.SaveChangesAsync();
                    GhiNhatKy("Đã cập nhật kho: " + kho.MaKho);
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
            
            ViewBag.MaNguyenLieu = new SelectList(_context.NguyenLieu, "MaNguyenLieu", "TenNguyenLieu", kho.MaNguyenLieu);
            return View(kho);
        }

        // --- DELETE (GET): Display confirmation page ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsThuKho()) return RedirectToAction("AccessDenied", "Home");
            if (id == null) return NotFound();

            var kho = await _context.Kho.Include(k => k.NguyenLieu)
                .FirstOrDefaultAsync(k => k.MaKho == id);
            
            return kho == null ? NotFound() : View(kho);
        }

        // --- DELETE (POST): Confirm deletion ---
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsThuKho()) return RedirectToAction("AccessDenied", "Home");
            var item = await _context.Kho.FindAsync(id);
            if (item != null)
            {
                _context.Kho.Remove(item);
                await _context.SaveChangesAsync();
                GhiNhatKy("Đã xóa mục khỏi kho: " + id);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}