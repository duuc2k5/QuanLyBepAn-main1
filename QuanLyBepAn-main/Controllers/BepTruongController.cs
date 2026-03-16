using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Data;
using QuanLyBepAn.Models;
using Microsoft.AspNetCore.Authorization;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace QuanLyBepAn.Controllers
{
    public class BepTruongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BepTruongController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsBepTruong()
        {
            return HttpContext.Session.GetString("Quyen") == "Bếp trưởng";
        }

        public async Task<IActionResult> Index(double? bmi = null)
        {
            if (!IsBepTruong()) return Unauthorized();
            ViewBag.Bmi = bmi;
            var ngls = await _context.NguyenLieu.OrderBy(n => n.TenNguyenLieu).ToListAsync();
            ViewBag.NguyenLieu = ngls;
            return View();
        }

        public class NutritionRequest
        {
            public int? RiceId { get; set; }
            public int RiceGrams { get; set; }
            public int? MeatId { get; set; }
            // meat in lạng (1 lạng = 100g)
            public int MeatLang { get; set; }
            public int? VegId { get; set; }
            public int VegGrams { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ComputeNutrition([FromBody] NutritionRequest req)
        {
            if (!IsBepTruong()) return Unauthorized();
            if (req == null) return BadRequest("Missing request");

            double totalCalo = 0;
            double totalProtein = 0;
            double totalLipid = 0;
            var warnings = new List<string>();

            async Task AddItem(int? id, double grams, string label)
            {
                if (!id.HasValue) return;
                var nl = await _context.NguyenLieu.FindAsync(id.Value);
                if (nl == null)
                {
                    warnings.Add($"Không tìm thấy nguyên liệu: {label}");
                    return;
                }
                if (!nl.Calo.HasValue || !nl.Dam.HasValue || !nl.Cali.HasValue)
                {
                    warnings.Add($"Nguyên liệu '{nl.TenNguyenLieu}' thiếu dữ liệu dinh dưỡng");
                }
                // assume stored per 100g
                double cal = (nl.Calo ?? 0) * grams / 100.0;
                double dam = (nl.Dam ?? 0) * grams / 100.0;
                double lip = (nl.Cali ?? 0) * grams / 100.0;
                totalCalo += cal;
                totalProtein += dam;
                totalLipid += lip;
            }

            // Rice
            await AddItem(req.RiceId, req.RiceGrams, "Cơm");
            // Meat (convert lạng to grams)
            await AddItem(req.MeatId, req.MeatLang * 100, "Thịt");
            // Veg
            await AddItem(req.VegId, req.VegGrams, "Rau");

            // Simple nutrient presence warning
            if (totalProtein < 5) warnings.Add("Cảnh báo: tổng protein rất thấp (<5g)");
            if (totalLipid < 2) warnings.Add("Cảnh báo: tổng lipid rất thấp (<2g)");

            return Ok(new
            {
                totalCalo = Math.Round(totalCalo, 2),
                totalProtein = Math.Round(totalProtein, 2),
                totalLipid = Math.Round(totalLipid, 2),
                warnings
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenu([FromBody] MenuCreateViewModel model)
        {
            if (!IsBepTruong()) return Unauthorized();
            if (model?.Items == null || model.Items.Count == 0) return BadRequest("No items");

            var td = new ThucDon { NgayApDung = DateTime.Now, TongCalo = 0 };
            _context.ThucDon.Add(td);
            await _context.SaveChangesAsync();

            double total = 0;
            foreach (var it in model.Items)
            {
                // create NguyenLieu for this item (if not exists by name)
                var nl = await _context.NguyenLieu.FirstOrDefaultAsync(n => n.TenNguyenLieu == it.Name);
                if (nl == null)
                {
                    nl = new NguyenLieu { TenNguyenLieu = it.Name, Calo = it.Calo, GiaTriDinhDuong = it.Calo };
                    _context.NguyenLieu.Add(nl);
                    await _context.SaveChangesAsync();
                }

                var item = new ThucDonItem { MaThucDon = td.MaThucDon, MaNguyenLieu = nl.MaNguyenLieu, SoLuong = 1 };
                _context.ThucDonItem.Add(item);
                total += it.Calo;
            }

            td.TongCalo = total;
            await _context.SaveChangesAsync();

            return Ok(new { id = td.MaThucDon });
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id, double? bmi)
        {
            // allow export for Bếp trưởng or Admin
            var q = HttpContext.Session.GetString("Quyen");
            if (q != "Bếp trưởng" && q != "Admin") return Unauthorized();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var td = await _context.ThucDon.FindAsync(id);
            if (td == null) return NotFound();

            var items = await _context.ThucDonItem.Where(d => d.MaThucDon == id).Include(d => d.NguyenLieu).ToListAsync();

            try
            {
                using var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Verdana", 14, XFontStyle.Bold);
                var fontSmall = new XFont("Verdana", 10, XFontStyle.Regular);

                double y = 40;
                if (bmi.HasValue)
                {
                    gfx.DrawString($"BMI: {bmi.Value:0.00}", fontSmall, XBrushes.DarkBlue, new XRect(40, y, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                    y += 22;
                }
                gfx.DrawString("Thực đơn", font, XBrushes.DarkRed, new XRect(40, y, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                y += 30;
                gfx.DrawString($"Ngày áp dụng: {td.NgayApDung:yyyy-MM-dd}", fontSmall, XBrushes.Black, new XRect(40, y, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                y += 25;

                foreach (var it in items)
                {
                    string line = $"- {it.NguyenLieu?.TenNguyenLieu}  (Calo: {it.NguyenLieu?.Calo ?? 0})";
                    gfx.DrawString(line, fontSmall, XBrushes.Black, new XRect(60, y, page.Width.Point - 120, 20), XStringFormats.TopLeft);
                    y += 18;
                    if (y > page.Height - 60)
                    {
                        // new page
                        page = doc.AddPage();
                        page.Size = PdfSharpCore.PageSize.A4;
                        gfx.Dispose();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;
                    }
                }

                y += 10;
                gfx.DrawString($"Tổng calo: {td.TongCalo}", fontSmall, XBrushes.Black, new XRect(40, y, page.Width.Point - 80, 20), XStringFormats.TopLeft);

                using var ms = new MemoryStream();
                doc.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", $"ThucDon_{td.MaThucDon}.pdf");
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.ToString());
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestExportPdf(double? bmi)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            try
            {
                using var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Verdana", 14, XFontStyle.Bold);
                var fontSmall = new XFont("Verdana", 10, XFontStyle.Regular);
                double y = 40;
                if (bmi.HasValue)
                {
                    gfx.DrawString($"BMI: {bmi.Value:0.00}", fontSmall, XBrushes.DarkBlue, new XRect(40, y, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                    y += 22;
                }
                gfx.DrawString("Thực đơn (Demo)", font, XBrushes.DarkRed, new XRect(40, y, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                y += 30;
                var sample = new[] { "Món demo 1 (Calo: 200)", "Món demo 2 (Calo: 150)", "Món demo 3 (Calo: 300)" };
                foreach (var s in sample)
                {
                    gfx.DrawString("- " + s, fontSmall, XBrushes.Black, new XRect(60, y, page.Width.Point - 120, 20), XStringFormats.TopLeft);
                    y += 18;
                }
                using var ms = new MemoryStream();
                doc.Save(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", "ThucDon_demo.pdf");
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.ToString());
            }
        }
    }
}
