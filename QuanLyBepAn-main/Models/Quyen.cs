// Models/Quyen.cs
using System.ComponentModel.DataAnnotations;

namespace QuanLyBepAn.Models
{
    [Display(Name = "Quyền")]
    public class Quyen
    {
        [Key]
        public int MaQuyen { get; set; }
        [Required]
        public required string TenQuyen { get; set; }
    }
}