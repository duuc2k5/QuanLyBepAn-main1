using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QuanLyBepAn.Data;
using QuanLyBepAn.Models;
using System;

namespace QuanLyBepAn.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected void GhiNhatKy(string hanhDong)
        {
            // Lấy MaNguoiDung từ Session, ép kiểu về int (mặc định là 0 nếu chưa có)
            var userIdStr = HttpContext.Session.GetString("MaNguoiDung");
            int userId = int.TryParse(userIdStr, out int id) ? id : 0;

            var log = new NhatKyHeThong
            {
                MaNguoiDung = userId,
                HanhDong = hanhDong,
                ThoiGian = DateTime.Now
            };

            _context.NhatKyHeThong.Add(log);
            _context.SaveChanges();
        }
    }
}