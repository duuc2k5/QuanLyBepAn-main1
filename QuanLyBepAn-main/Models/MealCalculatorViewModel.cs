using System.ComponentModel.DataAnnotations;

namespace QuanLyBepAn.Models
{
    public class MealCalculatorViewModel
    {
        [Required]
        [Range(1, 10000)]
        public int NumberOfChildren { get; set; }

        [Required]
        [Range(1, 10)]
        public int MealsPerDay { get; set; } = 1;

        [Required]
        [Range(0, 5000)]
        public int RicePerPortionGrams { get; set; } = 200;

        [Required]
        [Range(0, 1000)]
        // Meat per portion in lạng. Note: 1 lạng = 0.1 kg (100 grams)
        public int MeatPerPortionLang { get; set; } = 1;

        [Required]
        [Range(0, 5000)]
        public int VegPerPortionGrams { get; set; } = 100;

        // Results
        public int TotalPortions { get; set; }
        public double TotalRiceKg { get; set; }
        public double TotalMeatKg { get; set; }
        public double TotalVegKg { get; set; }
        public double TotalFoodKg { get; set; }
    }
}
