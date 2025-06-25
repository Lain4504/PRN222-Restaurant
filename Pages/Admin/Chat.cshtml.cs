using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Services.IService;
using System.Text.Json;
using System.Security.Claims;

namespace PRN222_Restaurant.Pages.Admin
{
    public class ChatModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

        public ChatModel(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        public int UserId { get; set; }

        public IActionResult OnGet()
        {
            // Get user ID from claims or session
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int claimUserId))
            {
                UserId = claimUserId;
            }
            else
            {
                // Fallback to session
                UserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            }

            // If no user ID, redirect to login
            if (UserId == 0)
            {
                return Redirect("/admin/login");
            }

            // Check if user is admin or staff
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
            {
                return Redirect("/admin/login");
            }

            return Page();
        }

        public async Task<IActionResult> OnGetGetCustomerConversationsAsync()
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (userId == 0)
                {
                    return BadRequest(new { error = "User not authenticated" });
                }

                var conversations = await _chatService.GetAllChatRoomsForAdminAsync();
                
                var result = conversations.Select(chat => new
                {
                    chatRoomId = chat.Id,
                    customerId = chat.CustomerId,
                    customerName = chat.Customer?.FullName ?? "Unknown Customer",
                    customerEmail = chat.Customer?.Email ?? "",
                    lastMessage = chat.Messages?.OrderByDescending(m => m.SentAt).FirstOrDefault()?.Content ?? "",
                    lastMessageAt = chat.LastMessageAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    unreadCount = chat.Messages?.Count(m => !m.IsRead && m.SenderId != userId) ?? 0,
                    isOnline = false, // You can implement online status tracking
                    status = chat.Status
                }).OrderByDescending(c => c.lastMessageAt).ToList();

                return new JsonResult(new { conversations = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to load conversations", details = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetGetMessagesAsync(int chatRoomId)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (userId == 0)
                {
                    return BadRequest(new { error = "User not authenticated" });
                }

                var messages = await _chatService.GetRecentMessagesAsync(chatRoomId, 50);
                
                var result = messages.Select(m => new
                {
                    id = m.Id,
                    chatRoomId = m.ChatRoomId,
                    senderId = m.SenderId,
                    senderName = !string.IsNullOrEmpty(m.Sender?.FullName)
                        ? m.Sender.FullName
                        : !string.IsNullOrEmpty(m.Sender?.Email)
                            ? m.Sender.Email
                            : $"User {m.SenderId}",
                    content = m.Content,
                    sentAt = m.SentAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    isRead = m.IsRead,
                    messageType = m.MessageType
                }).ToList();

                return new JsonResult(new { messages = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to load messages", details = ex.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostMarkMessagesAsReadAsync(int chatRoomId)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (userId == 0)
                {
                    return BadRequest(new { error = "User not authenticated" });
                }

                await _chatService.MarkMessagesAsReadAsync(chatRoomId, userId);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to mark messages as read", details = ex.Message });
            }
        }

        private int GetAuthenticatedUserId()
        {
            // Try to get from claims first
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int claimUserId))
            {
                return claimUserId;
            }

            // Fallback to session
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
    }
}
