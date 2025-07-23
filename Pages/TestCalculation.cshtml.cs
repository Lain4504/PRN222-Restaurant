using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Pages
{
    public class TestCalculationModel : PageModel
    {
        private readonly IPointsService _pointsService;

        public TestCalculationModel(IPointsService pointsService)
        {
            _pointsService = pointsService;
        }

        public PointsConfig Config { get; set; } = new();
        public List<CalculationExample> Examples { get; set; } = new();

        public async Task OnGetAsync()
        {
            Config = _pointsService.GetPointsConfig();
            
            // Create calculation examples
            var orderAmounts = new decimal[] { 100000, 250000, 500000, 750000, 1000000, 1500000, 2000000 };
            
            foreach (var amount in orderAmounts)
            {
                var pointsEarned = await _pointsService.CalculatePointsEarnedAsync(amount);
                var pointValue = pointsEarned * Config.PointValue;
                var returnRate = amount > 0 ? (pointValue / amount * 100) : 0;
                
                Examples.Add(new CalculationExample
                {
                    OrderAmount = amount,
                    PointsEarned = pointsEarned,
                    PointValue = pointValue,
                    ReturnRate = returnRate
                });
            }
        }
    }

    public class CalculationExample
    {
        public decimal OrderAmount { get; set; }
        public int PointsEarned { get; set; }
        public decimal PointValue { get; set; }
        public decimal ReturnRate { get; set; }
    }
}
