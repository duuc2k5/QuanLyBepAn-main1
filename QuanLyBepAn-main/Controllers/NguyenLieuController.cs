using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Data;
using QuanLyBepAn.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBepAn.Controllers
{
    public class NguyenLieuController : BaseController
    {
        public NguyenLieuController(ApplicationDbContext context) : base(context) { }

        // Only allow Bếp trưởng to access NguyenLieu management
        private bool IsBepTruong()
        {
            return HttpContext.Session.GetString("Quyen") == "Bếp trưởng";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsBepTruong()) return RedirectToAction("AccessDenied", "Home");
            return View(await _context.NguyenLieu.ToListAsync());
        }

        public IActionResult Create()
        {
            // Creation disabled: only details are allowed for NguyenLieu
            return RedirectToAction("AccessDenied", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenNguyenLieu,GiaTriDinhDuong,Calo,Dam,Cali")] NguyenLieu nguyenLieu)
        {
            // Creation via UI disabled
            return RedirectToAction("AccessDenied", "Home");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!IsBepTruong()) return RedirectToAction("AccessDenied", "Home");
            if (id == null) return NotFound();

            var nguyenLieu = await _context.NguyenLieu
                .FirstOrDefaultAsync(n => n.MaNguyenLieu == id);
            
            return nguyenLieu == null ? NotFound() : View(nguyenLieu);
        }

        // Editing disabled for NguyenLieu (read-only requirement)
        public IActionResult Edit(int? id)
        {
            return RedirectToAction("AccessDenied", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNguyenLieu,TenNguyenLieu,GiaTriDinhDuong,Calo,Dam,Cali")] NguyenLieu nguyenLieu)
        {
            // Editing disabled
            return RedirectToAction("AccessDenied", "Home");
        }

        // --- DELETE (GET): Display confirmation page ---
        public async Task<IActionResult> Delete(int? id)
        {
            // Deletion disabled
            return RedirectToAction("AccessDenied", "Home");
        }

        // --- DELETE (POST): Confirm deletion ---
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            // Deletion disabled
            return RedirectToAction("AccessDenied", "Home");
        }

        // API: trả về danh sách nguyên liệu dưới dạng JSON (dùng cho select trong JS)
        [HttpGet("/api/nguyenlieu/all")]
        public async Task<IActionResult> GetAllJson()
        {
            var list = await _context.NguyenLieu
                .Select(n => new { maNguyenLieu = n.MaNguyenLieu, tenNguyenLieu = n.TenNguyenLieu })
                .ToListAsync();
            return Json(list);
        }
    }
}