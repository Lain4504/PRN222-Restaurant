namespace PRN222_Restaurant.Services.IService
{
    public interface IStatisticsNotificationService
    {
        Task NotifyStatisticsUpdateAsync();
        Task NotifyNewOrderAsync(int orderId, decimal orderValue);
        Task NotifyOrderCompletedAsync(int orderId, decimal orderValue);
        Task NotifyNewCustomerAsync(int customerId);
    }
}
