using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Services.IService;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PRN222_Restaurant.Pages.Admin
{
    public class OrdersModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ITableService _tableService;
        private readonly ApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;
        private readonly IPointsService _pointsService;
        private const int DefaultPageSize = 10;

        public OrdersModel(IOrderService orderService, ITableService tableService, ApplicationDbContext context, NotificationHelper notificationHelper, IPointsService pointsService)
        {
            _orderService = orderService;
            _tableService = tableService;
            _context = context;
            _notificationHelper = notificationHelper;
            _pointsService = pointsService;
        }

        public PagedResult<Models.Order> OrdersResult { get; set; } = new PagedResult<Models.Order>();
        public List<Models.Order> Orders => OrdersResult.Items;
        public List<Table> AvailableTables { get; set; } = new List<Table>();
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public string? OrderStatus { get; set; }

        [BindProperty]
        public int OrderId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = DefaultPageSize;

        [BindProperty]
        public CreateOrderModel CreateOrderModel { get; set; } = new CreateOrderModel();
       
        public async Task OnGetAsync()
        {
            // Ensure valid pagination parameters
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = DefaultPageSize;

            // Get paginated orders
            OrdersResult = await _orderService.GetPagedOrdersAsync(CurrentPage, PageSize);
            // Load available tables and menu items for create order modal
            await LoadTablesAndMenuItems();
        }

        private async Task LoadTablesAndMenuItems()
        {
            AvailableTables = await _context.Tables.Where(t => t.Status == "Available").ToListAsync();
            MenuItems = await _context.MenuItems.Include(m => m.Category).ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            Console.WriteLine($"UpdateStatus called: OrderId={OrderId}, OrderStatus={OrderStatus}"); // Debug log

            if (string.IsNullOrEmpty(OrderStatus) || OrderId <= 0)
            {
                StatusMessage = $"Error: Invalid order information - OrderId: {OrderId}, Status: {OrderStatus}";
                return RedirectToPage("/admin/orders", new { CurrentPage, PageSize });
            }

            var success = await _orderService.UpdateOrderStatusAsync(OrderId, OrderStatus);
            // Cập nhật trạng thái bàn theo trạng thái đơn hàng
            if (success)
            {
                // Lấy đơn hàng để lấy TableId
                var order = await _orderService.GetOrderByIdAsync(OrderId);
                if (order != null && order.TableId.HasValue && order.TableId.Value > 0)
                {
                    string? newTableStatus = null;
                    if (OrderStatus == "Pending" || OrderStatus == "Preparing")
                        newTableStatus = "Reserved"; // sửa lại đúng trạng thái "Đã đặt"
                    else if (OrderStatus == "Served")
                        newTableStatus = "InUse"; // trạng thái "Đang sử dụng"
                    else if (OrderStatus == "Completed" || OrderStatus == "Cancelled")
                        newTableStatus = "Available";
                    if (newTableStatus != null)
                    {
                        await _tableService.ChangeStatusAsync(order.TableId.Value, newTableStatus);
                    }
                }

                // Create notification when order is completed
                if (OrderStatus == "Completed" && order != null && order.UserId.HasValue)
                {
                    await _notificationHelper.NotifyOrderCompletedAsync(order.UserId.Value, order.Id);
                }

                StatusMessage = $"Order #{OrderId} status updated to {OrderStatus}";
            }
            else
            {
                StatusMessage = $"Error: Failed to update order #{OrderId}";
            }
            return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
        }

        public async Task<IActionResult> OnPostConfirmRemainingPaymentAsync(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                StatusMessage = "Không tìm thấy đơn hàng.";
                return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
            }

            // Check existing payments
            var existingPayments = await _context.Payments
                .Where(p => p.OrderId == id)
                .ToListAsync();

            var totalPaid = existingPayments.Sum(p => p.Amount);
            var remainingAmount = order.TotalPrice - totalPaid;

            if (remainingAmount <= 0)
            {
                StatusMessage = "Đơn hàng đã được thanh toán đầy đủ.";
                return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
            }

            try
            {
                // Create payment record for remaining amount
                var finalPayment = new Payment
                {
                    OrderId = order.Id,
                    Amount = remainingAmount,
                    PaymentDate = DateTime.Now,
                    Status = "Paid",
                    PaymentType = "Final",
                    Method = "Cash" // Payment at restaurant counter
                };
                _context.Payments.Add(finalPayment);

                // Update order status to Paid
                var success = await _orderService.UpdateOrderStatusAsync(id, "Paid");

                if (success)
                {
                    await _context.SaveChangesAsync();

                    // Award points for completed order
                    if (order.UserId.HasValue)
                    {
                        await _pointsService.AwardPointsAsync(order.UserId.Value, order.Id, order.TotalPrice, "Order completion");

                        // Create notification for payment completion
                        await _notificationHelper.NotifyPaymentCompletedAsync(order.UserId.Value, order.Id, remainingAmount);
                    }

                    StatusMessage = $"Đã xác nhận thanh toán phần còn lại {remainingAmount:N0} VNĐ cho đơn hàng #{id}.";
                }
                else
                {
                    StatusMessage = $"Lỗi: Không thể cập nhật trạng thái đơn hàng #{id}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
            }

            return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
        }

        public async Task<IActionResult> OnPostCompleteOrderAsync(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                StatusMessage = "Không tìm thấy đơn hàng.";
                return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
            }

            // Update order status to completed
            var success = await _orderService.UpdateOrderStatusAsync(id, "Completed");

            if (success)
            {
                // Update table status to available
                if (order.TableId.HasValue)
                {
                    await _tableService.ChangeStatusAsync(order.TableId.Value, "Available");
                }

                // Create notification for order completion
                if (order.UserId.HasValue)
                {
                    await _notificationHelper.NotifyOrderCompletedAsync(order.UserId.Value, order.Id);
                }

                StatusMessage = $"Đơn hàng #{id} đã được hoàn thành thành công.";
            }
            else
            {
                StatusMessage = $"Lỗi: Không thể hoàn thành đơn hàng #{id}";
            }

            return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var success = await _orderService.CancelOrderAsync(id);

            if (success)
            {
                StatusMessage = $"Order #{id} has been cancelled";
            }
            else
            {
                StatusMessage = $"Error: Failed to cancel order #{id}";
            }
            return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
        }

        public async Task<IActionResult> OnPostCreateOrder()
        {
            // Load tables and menu items for the form if validation fails
            await LoadTablesAndMenuItems();

            // Validate table selection
            if (CreateOrderModel == null)
            {
                StatusMessage = "Error: Dữ liệu không hợp lệ";
                return Page();
            }

            if (CreateOrderModel.TableId <= 0)
            {
                ModelState.AddModelError("CreateOrderModel.TableId", "Vui lòng chọn bàn");
                StatusMessage = "Error: Vui lòng chọn bàn để tạo đơn hàng";
                return Page();
            }

            // Validate that at least one item has been selected
            if (CreateOrderModel.SelectedItems == null || !CreateOrderModel.SelectedItems.Any(i => i.Quantity > 0))
            {
                StatusMessage = "Error: Vui lòng chọn ít nhất một món ăn";
                return Page();
            }

            try
            {
                var order = new Models.Order
                {
                    TableId = CreateOrderModel.TableId,
                    OrderType = "Immediate",
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    Note = CreateOrderModel.Note,
                    OrderItems = new List<OrderItem>() // Initialize empty collection
                };

                // Đặt trạng thái bàn là 'Pending' (đang chờ xử lý) khi tạo đơn hàng mới
                var table = await _context.Tables.FirstOrDefaultAsync(t => t.Id == CreateOrderModel.TableId);
                if (table != null)
                {
                    table.Status = "Pending"; // Đặt đúng trạng thái là Pending khi vừa tạo đơn hàng
                    await _context.SaveChangesAsync();
                }

                var selectedItems = CreateOrderModel.SelectedItems
                    .Where(x => x.Quantity > 0)
                    .ToDictionary(x => x.MenuItemId, x => x.Quantity);

                var createdOrder = await _orderService.CreateImmediateOrderAsync(order, selectedItems);

                
                StatusMessage = $"Order #{createdOrder.Id} has been created successfully";
                return RedirectToPage("/admin/orders");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: Failed to create order - {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnGetOrderDetailsAsync(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return new JsonResult(new
            {
                order.Id,
                order.OrderType,
                order.Status,
                order.OrderDate,
                order.ReservationTime,
                order.TotalPrice,
                order.PointsUsed,
                order.Note,
                order.NumberOfGuests,
                Customer = order.User?.FullName ?? "Khách vãng lai",
                Table = order.Table?.TableNumber,
                Items = order.OrderItems.Select(item => new
                {
                    item.MenuItem.Name,
                    item.Quantity,
                    item.UnitPrice,
                    Total = item.Quantity * item.UnitPrice
                })
            });
        }
    }

    public class CreateOrderModel
    {
        public int TableId { get; set; }
        public string? Note { get; set; }
        public List<OrderItemModel> SelectedItems { get; set; } = new List<OrderItemModel>();
    }

    public class OrderItemModel
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}