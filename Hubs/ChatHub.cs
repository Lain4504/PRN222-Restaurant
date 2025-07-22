using Microsoft.AspNetCore.SignalR;
using PRN222_Restaurant.Services.IService;
using System.Security.Claims;

namespace PRN222_Restaurant.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, IUserService userService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("ChatHub OnConnectedAsync - User ID: {UserId}, Connection: {ConnectionId}", userId, Context.ConnectionId);

                if (userId.HasValue)
                {
                    await _chatService.AddUserConnectionAsync(userId.Value, Context.ConnectionId);
                    
                    // Join user to their personal group
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId.Value}");
                    
                    // If user is staff, join staff group
                    var userRole = GetUserRole();
                    if (userRole == "Staff" || userRole == "Admin")
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Staff");
                    }

                    // Notify others that user is online
                    await Clients.Others.SendAsync("UserOnline", userId.Value);
                    
                    _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId.Value, Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for connection {ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = GetUserId();
                if (userId.HasValue)
                {
                    await _chatService.RemoveUserConnectionAsync(Context.ConnectionId);
                    
                    // Notify others that user is offline
                    await Clients.Others.SendAsync("UserOffline", userId.Value);
                    
                    _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId.Value, Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for connection {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChatRoom(int chatRoomId)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("JoinChatRoom - User ID: {UserId}, Chat Room: {ChatRoomId}, Connection: {ConnectionId}", userId, chatRoomId, Context.ConnectionId);

                if (!userId.HasValue)
                {
                    _logger.LogWarning("JoinChatRoom - User not authenticated for connection {ConnectionId}", Context.ConnectionId);
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var chatRoom = await _chatService.GetChatRoomByIdAsync(chatRoomId);
                if (chatRoom == null)
                {
                    await Clients.Caller.SendAsync("Error", "Chat room not found");
                    return;
                }

                // Check if user has access to this chat room
                if (chatRoom.CustomerId != userId.Value && chatRoom.StaffId != userId.Value)
                {
                    var userRole = GetUserRole();
                    if (userRole != "Admin")
                    {
                        await Clients.Caller.SendAsync("Error", "Access denied");
                        return;
                    }
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoomId}");
                await Clients.Caller.SendAsync("JoinedChatRoom", chatRoomId);
                
                // Mark messages as read
                await _chatService.MarkMessagesAsReadAsync(chatRoomId, userId.Value);
                
                _logger.LogInformation("User {UserId} joined chat room {ChatRoomId}", userId.Value, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to join chat room");
            }
        }

        public async Task LeaveChatRoom(int chatRoomId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoomId}");
                await Clients.Caller.SendAsync("LeftChatRoom", chatRoomId);
                
                var userId = GetUserId();
                _logger.LogInformation("User {UserId} left chat room {ChatRoomId}", userId, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task SendMessage(int chatRoomId, string message)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "Message cannot be empty");
                    return;
                }

                var chatMessage = await _chatService.SendMessageAsync(chatRoomId, userId.Value, message.Trim());

                // Get sender information
                var sender = await _userService.GetUserByIdAsync(userId.Value);
                var senderName = sender?.FullName ?? sender?.Email ?? "Unknown";

                // Send message to all users in the chat room
                await Clients.Group($"ChatRoom_{chatRoomId}").SendAsync("ReceiveMessage", new
                {
                    Id = chatMessage.Id,
                    ChatRoomId = chatMessage.ChatRoomId,
                    SenderId = chatMessage.SenderId,
                    SenderName = senderName,
                    Content = chatMessage.Content,
                    SentAt = chatMessage.SentAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    MessageType = chatMessage.MessageType
                });

                // Notify staff about new messages
                var chatRoom = await _chatService.GetChatRoomByIdAsync(chatRoomId);
                if (chatRoom != null)
                {
                    // Notify all staff members about new messages (both from customers and other staff)
                    await Clients.Group("Staff").SendAsync("NewCustomerMessage", new
                    {
                        ChatRoomId = chatRoomId,
                        CustomerName = chatRoom.Customer?.FullName ?? "Unknown Customer",
                        Message = message.Trim(),
                        SentAt = chatMessage.SentAt,
                        SenderId = userId.Value,
                        SenderName = senderName
                    });
                }

                _logger.LogInformation("Message sent in chat room {ChatRoomId} by user {UserId}", chatRoomId, userId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        public async Task StartTyping(int chatRoomId)
        {
            try
            {
                var userId = GetUserId();
                if (userId.HasValue)
                {
                    await Clients.GroupExcept($"ChatRoom_{chatRoomId}", Context.ConnectionId)
                        .SendAsync("UserTyping", userId.Value, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartTyping for chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task StopTyping(int chatRoomId)
        {
            try
            {
                var userId = GetUserId();
                if (userId.HasValue)
                {
                    await Clients.GroupExcept($"ChatRoom_{chatRoomId}", Context.ConnectionId)
                        .SendAsync("UserTyping", userId.Value, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StopTyping for chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task AssignStaffToChat(int chatRoomId, int staffId)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                if (userRole != "Admin" && userRole != "Staff")
                {
                    await Clients.Caller.SendAsync("Error", "Access denied");
                    return;
                }

                await _chatService.AssignStaffToChatRoomAsync(chatRoomId, staffId);

                // Notify all users in the chat room
                await Clients.Group($"ChatRoom_{chatRoomId}").SendAsync("StaffAssigned", staffId);

                // Notify the assigned staff
                await Clients.Group($"User_{staffId}").SendAsync("AssignedToChat", chatRoomId);

                _logger.LogInformation("Staff {StaffId} assigned to chat room {ChatRoomId} by user {UserId}", staffId, chatRoomId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff to chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to assign staff");
            }
        }

        // Statistics real-time updates
        public async Task JoinStatisticsGroup()
        {
            try
            {
                var userRole = GetUserRole();
                if (userRole == "Admin" || userRole == "Staff")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Statistics");
                    _logger.LogInformation("User joined statistics group with connection {ConnectionId}", Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining statistics group for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        public async Task LeaveStatisticsGroup()
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Statistics");
                _logger.LogInformation("User left statistics group with connection {ConnectionId}", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving statistics group for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        private int? GetUserId()
        {
            // Try to get from JWT claims first
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int claimUserId))
            {
                return claimUserId;
            }

            // Fallback to session
            var sessionUserId = Context.GetHttpContext()?.Session.GetInt32("UserId");
            if (sessionUserId.HasValue)
            {
                return sessionUserId.Value;
            }

            // Try custom UserId claim
            var customUserIdClaim = Context.User?.FindFirst("UserId")?.Value;
            if (int.TryParse(customUserIdClaim, out int customUserId))
            {
                return customUserId;
            }

            return null;
        }

        private string? GetUserRole()
        {
            // Try to get from JWT claims first
            var roleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(roleClaim))
            {
                return roleClaim;
            }

            // Fallback to session
            return Context.GetHttpContext()?.Session.GetString("UserRole");
        }
    }
}
