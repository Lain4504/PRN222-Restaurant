using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Services.IService;
using System.Text.Json;
using System.Security.Claims;

namespace PRN222_Restaurant.Pages.Chat
{
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IChatService chatService, ILogger<IndexModel> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public int UserId { get; set; }

        public void OnGet()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                Response.Redirect("/admin/login");
                return;
            }

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

            // If still no user ID, redirect to login
            if (UserId == 0)
            {
                Response.Redirect("/admin/login");
                return;
            }
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostGetOrCreateChatRoomAsync()
        {
            try
            {
                // Get authenticated user ID
                var userId = GetAuthenticatedUserId();
                if (userId == 0)
                {
                    return BadRequest(new { error = "User not authenticated" });
                }
                var chatRoom = await _chatService.GetOrCreateChatRoomAsync(userId);
                
                if (chatRoom == null)
                {
                    return BadRequest(new { error = "Failed to create chat room" });
                }

                return new JsonResult(new { chatRoomId = chatRoom.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or getting chat room");
                return BadRequest(new { error = "Internal server error" });
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
                
                // Verify user has access to this chat room
                var chatRoom = await _chatService.GetChatRoomByIdAsync(chatRoomId);
                if (chatRoom == null || chatRoom.CustomerId != userId)
                {
                    return Forbid();
                }

                var messages = await _chatService.GetRecentMessagesAsync(chatRoomId, 50);
                
                var messageData = messages.Select(m => new
                {
                    id = m.Id,
                    chatRoomId = m.ChatRoomId,
                    senderId = m.SenderId,
                    senderName = m.Sender?.FullName ?? (m.SenderId == 0 ? "System" : "Unknown"),
                    content = m.Content,
                    sentAt = m.SentAt,
                    messageType = m.MessageType,
                    isRead = m.IsRead
                }).ToList();

                return new JsonResult(messageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for chat room {ChatRoomId}", chatRoomId);
                return BadRequest(new { error = "Failed to load messages" });
            }
        }

        public async Task<IActionResult> OnPostMarkMessagesAsReadAsync(int chatRoomId)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (userId == 0)
                {
                    return BadRequest(new { error = "User not authenticated" });
                }
                
                // Verify user has access to this chat room
                var chatRoom = await _chatService.GetChatRoomByIdAsync(chatRoomId);
                if (chatRoom == null || chatRoom.CustomerId != userId)
                {
                    return Forbid();
                }

                await _chatService.MarkMessagesAsReadAsync(chatRoomId, userId);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for chat room {ChatRoomId}", chatRoomId);
                return BadRequest(new { error = "Failed to mark messages as read" });
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
