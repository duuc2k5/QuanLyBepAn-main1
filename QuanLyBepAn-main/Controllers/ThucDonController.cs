using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Data;
using QuanLyBepAn.Hubs;
using QuanLyBepAn.Models;

namespace QuanLyBepAn.Controllers
{
    public class ThucDonController : BaseController
    {
        private readonly IHubContext<ThucDonHub> _hub;
        
        public ThucDonController(ApplicationDbContext context, IHubContext<ThucDonHub> hub) : base(context)
        {
            _hub = hub;
        }

        /// <summary>
        /// Kiểm tra quyền "Bếp trưởng"
        /// </summary>
        private bool IsBepTruong() => HttpContext.Session.GetString("Quyen") == "Bếp trưởng";

        /// <summary>
        /// Hiển thị danh sách thực đơn - chỉ Bếp trưởng mới có quyền xem
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (!IsBepTruong()) 
                return RedirectToAction("AccessDenied", "Home");
            
            var ds = await _context.ThucDon
                .OrderByDescending(t => t.NgayApDung)
                .ToListAsync();
            
            return View(ds);
        }

        /// <summary>
        /// API: Lấy danh sách các item (nguyên liệu) của một thực đơn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetItems(int id)
        {
            if (!IsBepTruong()) 
                return Unauthorized();
            
            try
            {
                var items = await _context.ThucDonItem
                    .Where(i => i.MaThucDon == id)
                    .Include(i => i.NguyenLieu)
                    .ToListAsync();

                // Kèm theo số lượng tồn kho từ bảng Kho
                var result = items.Select(i => new {
                    i.MaThucDonItem,
                    i.MaNguyenLieu,
                    TenNguyenLieu = i.NguyenLieu?.TenNguyenLieu ?? "N/A",
                    i.SoLuong,
                    Calo = i.NguyenLieu?.Calo ?? 0,
                    SoLuongTon = _context.Kho
                        .FirstOrDefault(k => k.MaNguyenLieu == i.MaNguyenLieu)?.SoLuongTon ?? 0
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Thêm nguyên liệu vào thực đơn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddItem([FromForm] int maThucDon, [FromForm] int maNguyenLieu, [FromForm] double soLuong)
        {
            if (!IsBepTruong()) 
                return Unauthorized();

            if (soLuong <= 0)
                return BadRequest(new { error = "Số lượng phải lớn hơn 0" });

            try
            {
                // Kiểm tra thực đơn có tồn tại
                var thucDon = await _context.ThucDon.FindAsync(maThucDon);
                if (thucDon == null)
                    return NotFound(new { error = "Thực đơn không tồn tại" });

                // Kiểm tra nguyên liệu có tồn tại
                var nguyenLieu = await _context.NguyenLieu.FindAsync(maNguyenLieu);
                if (nguyenLieu == null)
                    return NotFound(new { error = "Nguyên liệu không tồn tại" });

                var item = new ThucDonItem 
                { 
                    MaThucDon = maThucDon, 
                    MaNguyenLieu = maNguyenLieu, 
                    SoLuong = soLuong 
                };

                // Cập nhật kho - giảm số lượng tồn
                var kho = await _context.Kho.FirstOrDefaultAsync(k => k.MaNguyenLieu == maNguyenLieu);
                if (kho == null)
                {
                    kho = new Kho 
                    { 
                        MaNguyenLieu = maNguyenLieu, 
                        SoLuongTon = -soLuong // Số âm khi chưa có hàng
                    };
                    _context.Kho.Add(kho);
                }
                else
                {
                    kho.SoLuongTon -= soLuong;
                    _context.Kho.Update(kho);
                }

                // Cập nhật TongCalo
                var calos = await _context.ThucDonItem
                    .Where(i => i.MaThucDon == maThucDon)
                    .Include(i => i.NguyenLieu)
                    .ToListAsync();

                double totalCalo = calos.Sum(i => (i.NguyenLieu?.Calo ?? 0) * i.SoLuong)
                    + (nguyenLieu.Calo ?? 0) * soLuong;

                thucDon.TongCalo = totalCalo;
                _context.ThucDon.Update(thucDon);

                _context.ThucDonItem.Add(item);
                await _context.SaveChangesAsync();

                GhiNhatKy($"Thêm nguyên liệu '{nguyenLieu.TenNguyenLieu}' vào thực đơn {maThucDon}");

                var payload = new 
                { 
                    action = "add", 
                    item, 
                    kho = new { kho.MaKho, kho.MaNguyenLieu, kho.SoLuongTon },
                    thucDon = new { thucDon.MaThucDon, thucDon.TongCalo }
                };
                
                await _hub.Clients.All.SendAsync("MenuUpdated", payload);
                
                return Ok(new { success = true, item, kho, thucDon });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Chỉnh sửa số lượng nguyên liệu trong thực đơn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EditItem([FromForm] int id, [FromForm] double soLuong)
        {
            if (!IsBepTruong()) 
                return Unauthorized();

            if (soLuong <= 0)
                return BadRequest(new { error = "Số lượng phải lớn hơn 0" });

            try
            {
                var item = await _context.ThucDonItem
                    .Include(i => i.NguyenLieu)
                    .FirstOrDefaultAsync(i => i.MaThucDonItem == id);

                if (item == null)
                    return NotFound(new { error = "Không tìm thấy item" });

                var oldSoLuong = item.SoLuong;
                var delta = soLuong - oldSoLuong; // Dương = tăng => giảm kho

                // Cập nhật kho theo chênh lệch
                var kho = await _context.Kho.FirstOrDefaultAsync(k => k.MaNguyenLieu == item.MaNguyenLieu);
                if (kho == null)
                {
                    kho = new Kho 
                    { 
                        MaNguyenLieu = item.MaNguyenLieu, 
                        SoLuongTon = -delta 
                    };
                    _context.Kho.Add(kho);
                }
                else
                {
                    kho.SoLuongTon -= delta;
                    _context.Kho.Update(kho);
                }

                item.SoLuong = soLuong;
                _context.ThucDonItem.Update(item);

                // Cập nhật TongCalo
                var thucDon = await _context.ThucDon.FindAsync(item.MaThucDon);
                if (thucDon != null)
                {
                    var calos = await _context.ThucDonItem
                        .Where(i => i.MaThucDon == item.MaThucDon)
                        .Include(i => i.NguyenLieu)
                        .ToListAsync();

                    double totalCalo = calos
                        .Where(i => i.MaThucDonItem != id)
                        .Sum(i => (i.NguyenLieu?.Calo ?? 0) * i.SoLuong)
                        + (item.NguyenLieu?.Calo ?? 0) * soLuong;

                    thucDon.TongCalo = totalCalo;
                    _context.ThucDon.Update(thucDon);
                }

                await _context.SaveChangesAsync();

                GhiNhatKy($"Chỉnh sửa số lượng '{item.NguyenLieu?.TenNguyenLieu}' từ {oldSoLuong} thành {soLuong}");

                var payload = new 
                { 
                    action = "edit", 
                    item, 
                    kho = new { kho.MaKho, kho.MaNguyenLieu, kho.SoLuongTon },
                    thucDon = new { thucDon?.MaThucDon, thucDon?.TongCalo }
                };
                
                await _hub.Clients.All.SendAsync("MenuUpdated", payload);
                
                return Ok(new { success = true, item, kho, thucDon });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa nguyên liệu khỏi thực đơn (trả lại kho)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteItem([FromForm] int id)
        {
            if (!IsBepTruong()) 
                return Unauthorized();

            try
            {
                var item = await _context.ThucDonItem
                    .Include(i => i.NguyenLieu)
                    .FirstOrDefaultAsync(i => i.MaThucDonItem == id);

                if (item == null)
                    return NotFound(new { error = "Không tìm thấy item" });

                // Trả lại kho khi xóa item
                var kho = await _context.Kho.FirstOrDefaultAsync(k => k.MaNguyenLieu == item.MaNguyenLieu);
                if (kho != null)
                {
                    kho.SoLuongTon += item.SoLuong;
                    _context.Kho.Update(kho);
                }

                var thucDonId = item.MaThucDon;
                var nguyenLieuName = item.NguyenLieu?.TenNguyenLieu ?? "N/A";

                _context.ThucDonItem.Remove(item);

                // Cập nhật TongCalo
                var thucDon = await _context.ThucDon.FindAsync(thucDonId);
                if (thucDon != null)
                {
                    var calos = await _context.ThucDonItem
                        .Where(i => i.MaThucDon == thucDonId)
                        .Include(i => i.NguyenLieu)
                        .ToListAsync();

                    thucDon.TongCalo = calos.Sum(i => (i.NguyenLieu?.Calo ?? 0) * i.SoLuong);
                    _context.ThucDon.Update(thucDon);
                }

                await _context.SaveChangesAsync();

                GhiNhatKy($"Xóa nguyên liệu '{nguyenLieuName}' khỏi thực đơn {thucDonId}");

                var payload = new 
                { 
                    action = "delete", 
                    id, 
                    maNguyenLieu = item.MaNguyenLieu,
                    kho = kho == null ? null : new { kho.MaKho, kho.MaNguyenLieu, kho.SoLuongTon },
                    thucDon = new { thucDon?.MaThucDon, thucDon?.TongCalo }
                };
                
                await _hub.Clients.All.SendAsync("MenuUpdated", payload);
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo thực đơn mới (ngày hiện tại)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            if (!IsBepTruong()) 
                return Unauthorized();

            try
            {
                var td = new ThucDon 
                { 
                    NgayApDung = DateTime.Now, 
                    TongCalo = 0 
                };

                _context.ThucDon.Add(td);
                await _context.SaveChangesAsync();

                GhiNhatKy($"Tạo thực đơn mới cho ngày {td.NgayApDung:yyyy-MM-dd}");

                await _hub.Clients.All.SendAsync("MenuUpdated", new { action = "created", td });
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Chỉnh sửa ngày áp dụng của thực đơn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] int id, [FromForm] DateTime ngayApDung)
        {
            if (!IsBepTruong()) 
                return Unauthorized();

            try
            {
                var td = await _context.ThucDon.FindAsync(id);
                if (td == null)
                    return NotFound(new { error = "Thực đơn không tồn tại" });

                var oldDate = td.NgayApDung;
                td.NgayApDung = ngayApDung;
                _context.ThucDon.Update(td);
                await _context.SaveChangesAsync();

                GhiNhatKy($"Cập nhật ngày thực đơn {id} từ {oldDate:yyyy-MM-dd} thành {ngayApDung:yyyy-MM-dd}");

                await _hub.Clients.All.SendAsync("MenuUpdated", new { action = "edited", td });
                
                return Ok(new { success = true, td });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa thực đơn và các item của nó, trả lại kho
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            if (!IsBepTruong()) 
                return Unauthorized();

            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var td = await _context.ThucDon.FindAsync(id);
                    if (td == null)
                        return NotFound(new { error = "Thực đơn không tồn tại" });

                    // Lấy tất cả item để trả lại kho
                    var items = await _context.ThucDonItem
                        .Where(i => i.MaThucDon == id)
                        .ToListAsync();

                    foreach (var item in items)
                    {
                        var kho = await _context.Kho.FirstOrDefaultAsync(k => k.MaNguyenLieu == item.MaNguyenLieu);
                        if (kho != null)
                        {
                            kho.SoLuongTon += item.SoLuong;
                            _context.Kho.Update(kho);
                        }
                    }

                    // Xóa item rồi xóa thực đơn
                    _context.ThucDonItem.RemoveRange(items);
                    _context.ThucDon.Remove(td);
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();

                    GhiNhatKy($"Xóa thực đơn {id} (ngày: {td.NgayApDung:yyyy-MM-dd}) và {items.Count} nguyên liệu");
                    
                    await _hub.Clients.All.SendAsync("MenuUpdated", new { action = "deleted", id });
                    
                    return Ok(new { success = true });
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return BadRequest(new { error = ex.Message });
                }
            }
        }

        /// <summary>
        /// API: Lấy tất cả nguyên liệu để dùng cho dropdown
        /// </summary>
        [HttpGet("/api/thucdon/nguyenlieu")]
        public async Task<IActionResult> GetAllNguyenLieu()
        {
            try
            {
                var list = await _context.NguyenLieu
                    .Where(n => n.TenNguyenLieu != null)
                    .Select(n => new 
                    { 
                        maNguyenLieu = n.MaNguyenLieu, 
                        tenNguyenLieu = n.TenNguyenLieu,
                        calo = n.Calo ?? 0
                    })
                    .OrderBy(x => x.tenNguyenLieu)
                    .ToListAsync();

                return Json(list);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
