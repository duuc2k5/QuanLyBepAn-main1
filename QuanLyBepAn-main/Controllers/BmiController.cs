using Microsoft.AspNetCore.Mvc;

namespace QuanLyBepAn.Controllers
{
    public class BmiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate(double heightCm, double weightKg)
        {
            if (heightCm <= 0 || weightKg <= 0)
            {
                ViewData["Error"] = "Vui lòng nhập chiều cao và cân nặng hợp lệ.";
                return View("Index");
            }

            var h = heightCm / 100.0;
            var bmi = weightKg / (h * h);

            string category;
            if (bmi < 18.5) category = "Thiếu cân";
            else if (bmi < 25) category = "Bình thường";
            else if (bmi < 30) category = "Thừa cân";
            else category = "Béo phì";

            ViewData["Bmi"] = bmi;
            ViewData["Category"] = category;
            ViewData["Height"] = heightCm;
            ViewData["Weight"] = weightKg;

            return View("Index");
        }
    }
}
