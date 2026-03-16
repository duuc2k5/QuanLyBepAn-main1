// Models/Kho.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBepAn.Models
{
    public class Kho
    {
        [Key]
        public int MaKho { get; set; }
        public int MaNguyenLieu { get; set; }
        [ForeignKey("MaNguyenLieu")]
        public NguyenLieu? NguyenLieu { get; set; }
        public double SoLuongTon { get; set; }
    }
}