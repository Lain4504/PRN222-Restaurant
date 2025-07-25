﻿using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Repositories.IRepository;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _notificationRepository.GetAllAsync();
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _notificationRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        return await _notificationRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId)
    {
        return await _notificationRepository.GetUnreadByUserIdAsync(userId);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(int userId)
    {
        return await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
    }

    public async Task<PagedResult<Notification>> GetPagedByUserIdAsync(int userId, int page, int pageSize)
    {
        return await _notificationRepository.GetPagedByUserIdAsync(userId, page, pageSize);
    }

    public async Task AddAsync(Notification notification)
    {
        await _notificationRepository.AddAsync(notification);
    }

    public async Task UpdateAsync(Notification notification)
    {
        await _notificationRepository.UpdateAsync(notification);
    }

    public async Task DeleteAsync(int id)
    {
        await _notificationRepository.DeleteAsync(id);
    }

    public async Task MarkAsReadAsync(int id)
    {
        await _notificationRepository.MarkAsReadAsync(id);
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task<IEnumerable<Notification>> GetLatestByUserIdAsync(int userId, int count = 5)
    {
        return await _notificationRepository.GetLatestByUserIdAsync(userId, count);
    }

    // Business logic methods
    public async Task CreateReservationNotificationAsync(int userId, int reservationId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Đặt bàn thành công",
            Message = "Bạn đã đặt bàn thành công. Chúng tôi sẽ liên hệ với bạn sớm nhất.",
            Type = "Success",
            RelatedUrl = "/order-list", // Chuyển đến trang danh sách đơn hàng thay vì reservation detail
            RelatedId = reservationId,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreateOrderNotificationAsync(int userId, int orderId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Đơn hàng đã được tạo",
            Message = "Đơn hàng của bạn đã được tạo thành công và đang được xử lý.",
            Type = "Success",
            RelatedUrl = $"/order-detail/{orderId}",
            RelatedId = orderId,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreatePaymentSuccessNotificationAsync(int userId, int orderId, decimal amount)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Thanh toán cọc thành công",
            Message = $"Bạn đã thanh toán cọc thành công {amount:N0} VNĐ cho đơn hàng #{orderId}. Vui lòng thanh toán phần còn lại tại quầy.",
            Type = "Success",
            RelatedUrl = $"/order-detail/{orderId}",
            RelatedId = orderId,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreatePaymentFailedNotificationAsync(int userId, int orderId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Thanh toán thất bại",
            Message = $"Thanh toán cho đơn hàng #{orderId} đã thất bại. Vui lòng thử lại.",
            Type = "Error",
            RelatedUrl = $"/order-list",
            RelatedId = orderId,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreateOrderCompletedNotificationAsync(int userId, int orderId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Đơn hàng hoàn thành",
            Message = "Cảm ơn bạn đã sử dụng dịch vụ! Hãy đánh giá trải nghiệm của bạn.",
            Type = "Info",
            RelatedUrl = $"/order-detail/{orderId}",
            RelatedId = orderId,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreatePointsEarnedNotificationAsync(int userId, int points, int orderId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Điểm thưởng mới",
            Message = $"Bạn đã nhận được {points} điểm từ đơn hàng #{orderId}. Tổng điểm hiện tại của bạn đã được cập nhật!",
            Type = "Success",
            RelatedUrl = $"/order-detail/{orderId}",
            RelatedId = orderId,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreatePointsRedeemedNotificationAsync(int userId, int points, decimal discount, int orderId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Điểm đã sử dụng",
            Message = $"Bạn đã sử dụng {points} điểm để được giảm giá {discount:C} cho đơn hàng #{orderId}.",
            Type = "Info",
            RelatedUrl = "/points-history",
            RelatedId = orderId,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreateWelcomeBonusNotificationAsync(int userId, int points)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Chào mừng bạn đến với chương trình thành viên!",
            Message = $"Bạn đã nhận được {points} điểm chào mừng! Hãy sử dụng điểm để được giảm giá trong các đơn hàng tiếp theo.",
            Type = "Success",
            RelatedUrl = "/points-history",
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreateOrderAutoCancelledNotificationAsync(int userId, int orderId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Đơn hàng bị hủy tự động",
            Message = $"Đơn hàng #{orderId} của bạn đã bị hủy do chưa thanh toán trong vòng 10 phút.",
            CreatedAt = DateTime.Now,
            IsRead = false
        };
        await AddAsync(notification);
    }

    public async Task CreatePaymentCompletedNotificationAsync(int userId, int orderId, decimal amount)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Thanh toán hoàn tất",
            Message = $"Bạn đã thanh toán thành công {amount:N0} VNĐ cho đơn hàng #{orderId}. Đơn hàng của bạn đã được thanh toán đầy đủ.",
            Type = "Success",
            RelatedUrl = $"/order-detail/{orderId}",
            CreatedAt = DateTime.Now,
            IsRead = false
        };
        await AddAsync(notification);
    }
}