using System.Collections.Generic;

namespace QuanLyBepAn.Models
{
    public class MenuItemDto
    {
        public string Name { get; set; } = string.Empty;
        public double Calo { get; set; }
    }

    public class MenuCreateViewModel
    {
        public List<MenuItemDto> Items { get; set; } = new List<MenuItemDto>();
    }
}
