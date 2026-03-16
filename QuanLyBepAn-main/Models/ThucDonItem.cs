using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBepAn.Models
{
    public class ThucDonItem
    {
        [Key]
        public int MaThucDonItem { get; set; }

        public int MaThucDon { get; set; }
        [ForeignKey("MaThucDon")]
        public ThucDon? ThucDon { get; set; }

        public int MaNguyenLieu { get; set; }
        [ForeignKey("MaNguyenLieu")]
        public NguyenLieu? NguyenLieu { get; set; }

        // Số lượng nguyên liệu cần cho món này (đơn vị tùy theo ứng dụng)
        public double SoLuong { get; set; }
    }
}