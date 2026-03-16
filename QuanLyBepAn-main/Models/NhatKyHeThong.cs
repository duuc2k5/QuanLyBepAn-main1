// Models/NhatKyHeThong.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBepAn.Models
{
    public class NhatKyHeThong
    {
        [Key]
        [Display(Name = "Mã Log")]
        public int MaLog { get; set; }
        
        [Display(Name = "Mã Người Dùng")]
        public int MaNguoiDung { get; set; }
        
        [ForeignKey("MaNguoiDung")]
        [Display(Name = "Người Dùng")]
        public NguoiDung? NguoiDung { get; set; }
        
        [Display(Name = "Hành Động")]
        public required string HanhDong { get; set; }
        
        [Display(Name = "Thời Gian")]
        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}