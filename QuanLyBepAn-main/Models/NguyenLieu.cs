// Models/NguyenLieu.cs
using System.ComponentModel.DataAnnotations;

namespace QuanLyBepAn.Models
{
    public class NguyenLieu
    {
        [Key]
        public int MaNguyenLieu { get; set; }
        public required string TenNguyenLieu { get; set; }
        public double GiaTriDinhDuong { get; set; }

        // Trường dinh dưỡng: lưu vào DB
        public double? Calo { get; set; }

        public double? Dam { get; set; }

        public double? Cali { get; set; }
    }
}