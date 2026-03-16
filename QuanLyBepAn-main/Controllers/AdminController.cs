using Microsoft.AspNetCore.Mvc;
using QuanLyBepAn.Models;

namespace QuanLyBepAn.Controllers
{
    public class AdminController : Controller
    {
        private bool IsAdmin() => HttpContext.Session.GetString("Quyen") == "Admin";

        [HttpGet]
        public IActionResult MealCalculator()
        {
            if (!IsAdmin()) return Unauthorized();
            var vm = new MealCalculatorViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MealCalculator(MealCalculatorViewModel vm)
        {
            if (!IsAdmin()) return Unauthorized();
            if (!ModelState.IsValid) return View(vm);

            vm.TotalPortions = vm.NumberOfChildren * vm.MealsPerDay;
            vm.TotalRiceKg = vm.TotalPortions * vm.RicePerPortionGrams / 1000.0;
            // Meat input is in lạng (100g). Convert to kg: lạng * 0.1 = kg per portion
            vm.TotalMeatKg = vm.TotalPortions * vm.MeatPerPortionLang * 0.1;
            vm.TotalVegKg = vm.TotalPortions * vm.VegPerPortionGrams / 1000.0;
            vm.TotalFoodKg = vm.TotalRiceKg + vm.TotalMeatKg + vm.TotalVegKg;

            return View(vm);
        }
    }
}
