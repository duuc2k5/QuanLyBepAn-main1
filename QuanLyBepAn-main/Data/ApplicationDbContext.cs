// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Models; // Đảm bảo namespace trùng với tên dự án của bạn

namespace QuanLyBepAn.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng (DbSet) tương ứng với 6 model bạn đã tạo
        public DbSet<Quyen> Quyen { get; set; }
        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<NhatKyHeThong> NhatKyHeThong { get; set; }
        public DbSet<NguyenLieu> NguyenLieu { get; set; }
        public DbSet<ThucDon> ThucDon { get; set; }
        public DbSet<ThucDonItem> ThucDonItem { get; set; }
        public DbSet<Kho> Kho { get; set; }
    }
}