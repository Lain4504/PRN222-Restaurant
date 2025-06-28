using Microsoft.AspNetCore.SignalR;
using PRN222_Restaurant.Hubs;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service
{
    public class StatisticsNotificationService : IStatisticsNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<StatisticsNotificationService> _logger;

        public StatisticsNotificationService(
            IHubContext<ChatHub> hubContext,
            IStatisticsService statisticsService,
            ILogger<StatisticsNotificationService> logger)
        {
            _hubContext = hubContext;
            _statisticsService = statisticsService;
            _logger = logger;
        }

        public async Task NotifyStatisticsUpdateAsync()
        {
            try
            {
                // Get updated statistics
                var statistics = await _statisticsService.GetStatisticsAsync();
                
                // Send to all admin/staff users
                await _hubContext.Clients.Group("Statistics").SendAsync("StatisticsUpdated", statistics);
                
                _logger.LogInformation("Statistics update notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending statistics update notification");
            }
        }

        public async Task NotifyNewOrderAsync(int orderId, decimal orderValue)
        {
            try
            {
                await _hubContext.Clients.Group("Statistics").SendAsync("NewOrder", new
                {
                    OrderId = orderId,
                    OrderValue = orderValue,
                    Timestamp = DateTime.Now
                });

                // Also trigger a full statistics update
                await NotifyStatisticsUpdateAsync();
                
                _logger.LogInformation("New order notification sent for order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new order notification for order {OrderId}", orderId);
            }
        }

        public async Task NotifyOrderCompletedAsync(int orderId, decimal orderValue)
        {
            try
            {
                await _hubContext.Clients.Group("Statistics").SendAsync("OrderCompleted", new
                {
                    OrderId = orderId,
                    OrderValue = orderValue,
                    Timestamp = DateTime.Now
                });

                // Trigger a full statistics update since completed orders affect revenue
                await NotifyStatisticsUpdateAsync();
                
                _logger.LogInformation("Order completed notification sent for order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order completed notification for order {OrderId}", orderId);
            }
        }

        public async Task NotifyNewCustomerAsync(int customerId)
        {
            try
            {
                await _hubContext.Clients.Group("Statistics").SendAsync("NewCustomer", new
                {
                    CustomerId = customerId,
                    Timestamp = DateTime.Now
                });

                // Trigger a statistics update to reflect new customer count
                await NotifyStatisticsUpdateAsync();
                
                _logger.LogInformation("New customer notification sent for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new customer notification for customer {CustomerId}", customerId);
            }
        }
    }
}
