using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Rất quan trọng

namespace QuanLyBepAn.Models
{
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }
        
        [Display(Name = "Tên Đăng Nhập")]
        public required string TenDangNhap { get; set; }
        
        [Display(Name = "Mật Khẩu")]
        public required string MatKhauHash { get; set; }
        
        [Display(Name = "Quyền")]
        public int MaQuyen { get; set; }

        // Thêm dòng này để EF không tự suy diễn tên cột sai
        [ForeignKey("MaQuyen")]
        [Display(Name = "Quyền")]
        public virtual Quyen? Quyen { get; set; }
    }
}