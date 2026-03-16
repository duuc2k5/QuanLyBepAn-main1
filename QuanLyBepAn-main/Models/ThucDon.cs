// Models/ThucDon.cs
using System.ComponentModel.DataAnnotations;

namespace QuanLyBepAn.Models
{
    public class ThucDon
    {
        [Key]
        public int MaThucDon { get; set; }
        public DateTime NgayApDung { get; set; }
        public double TongCalo { get; set; }
    }
}