using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Group03_Kindergarten_Suggestion_System_Project.Data;
using Group03_Kindergarten_Suggestion_System_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Group03_Kindergarten_Suggestion_System_Project.Pages.Ratings
{
    public class ViewRatingsModel : PageModel
    {
        private readonly KindergartenSSDatabase _context;

        public ViewRatingsModel(KindergartenSSDatabase context)
        {
            _context = context;
        }
        public double LearningProgramRating { get; set; }
        public double FacilitiesRating { get; set; }
        public double ExtracurricularRating { get; set; }
        public double TeachersStaffRating { get; set; }
        public double HygieneNutritionRating { get; set; }
        public List<SchoolRating> Ratings { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid schoolId, DateTime? fromDate, DateTime? toDate)
        {
            fromDate ??= new DateTime(2000, 1, 1);
            toDate ??= DateTime.UtcNow;

            Ratings = await _context.SchoolRatings
                .Where(r => r.SchoolId == schoolId && r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
                .Include(r => r.Parent)
                .ToListAsync();

            AverageRating = Ratings.Any() ? Ratings.Average(r => r.GetAvgRating()) : 0;
            TotalReviews = Ratings.Count;
            if (TotalReviews > 0)
            {
                double totalPossibleStars = TotalReviews * 5.0; // Tổng số sao tối đa có thể đạt được

                LearningProgramRating = Ratings.Sum(r => r.Rating1) / totalPossibleStars * 5;
                FacilitiesRating = Ratings.Sum(r => r.Rating2) / totalPossibleStars * 5;
                ExtracurricularRating = Ratings.Sum(r => r.Rating3) / totalPossibleStars * 5;
                TeachersStaffRating = Ratings.Sum(r => r.Rating4) / totalPossibleStars * 5;
                HygieneNutritionRating = Ratings.Sum(r => r.Rating5) / totalPossibleStars * 5;
            }
            else
            {
                LearningProgramRating = FacilitiesRating = ExtracurricularRating = TeachersStaffRating = HygieneNutritionRating = 0;
            }
            return Page();
        }
    }
}
