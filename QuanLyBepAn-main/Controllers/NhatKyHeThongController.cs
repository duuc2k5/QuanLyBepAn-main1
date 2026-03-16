using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Data;
using System.Threading.Tasks;

namespace QuanLyBepAn.Controllers
{
    public class NhatKyHeThongController : BaseController
    {
        public NhatKyHeThongController(ApplicationDbContext context) : base(context) { }

        private bool IsAdmin() => HttpContext.Session.GetString("Quyen") == "Admin";

        // --- INDEX: Hiển thị nhật ký hệ thống ---
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");

            var logs = await _context.NhatKyHeThong
                .Include(l => l.NguoiDung)
                .OrderByDescending(l => l.ThoiGian)
                .ToListAsync();
            
            return View(logs);
        }
    }
}
