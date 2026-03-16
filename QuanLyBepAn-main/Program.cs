using Microsoft.EntityFrameworkCore;
using QuanLyBepAn.Data;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký dịch vụ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session tồn tại 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Quan trọng: Kích hoạt Session
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hubs
app.MapHub<QuanLyBepAn.Hubs.ThucDonHub>("/thucdonHub");

// Seed roles 'Bếp trưởng' và 'Thủ kho' nếu chưa có
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Tạo database/migrations nếu cần
        db.Database.Migrate();
        if (!db.Quyen.Any(q => q.TenQuyen == "Bếp trưởng"))
        {
            db.Quyen.Add(new QuanLyBepAn.Models.Quyen { TenQuyen = "Bếp trưởng" });
        }
        if (!db.Quyen.Any(q => q.TenQuyen == "Thủ kho"))
        {
            db.Quyen.Add(new QuanLyBepAn.Models.Quyen { TenQuyen = "Thủ kho" });
        }
        db.SaveChanges();
    }
    catch (Exception ex)
    {
        // Không ném lỗi tại startup; ghi nhật ký để kiểm tra
        System.Diagnostics.Debug.WriteLine("Seed roles failed: " + ex.Message);
    }
}

app.Run();