using Microsoft.AspNetCore.Mvc;
using QuanLyBepAn.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace QuanLyBepAn.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("User")))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}