using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserService _userService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IChatRepository chatRepository, IUserService userService, ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _userService = userService;
            _logger = logger;
        }

        // ChatRoom operations
        public async Task<IEnumerable<ChatRoom>> GetAllChatRoomsAsync()
        {
            return await _chatRepository.GetAllChatRoomsAsync();
        }

        public async Task<List<ChatRoom>> GetAllChatRoomsForAdminAsync()
        {
            return await _chatRepository.GetAllChatRoomsForAdminAsync();
        }

        public async Task<ChatRoom?> GetChatRoomByIdAsync(int id)
        {
            return await _chatRepository.GetChatRoomByIdAsync(id);
        }

        public async Task<ChatRoom?> GetOrCreateChatRoomAsync(int customerId)
        {
            // Check if customer already has an active chat room
            var existingRoom = await _chatRepository.GetChatRoomByCustomerIdAsync(customerId);
            if (existingRoom != null)
            {
                return existingRoom;
            }

            // Create new chat room
            var customer = await _userService.GetUserByIdAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException("Customer not found", nameof(customerId));
            }

            var chatRoom = new ChatRoom
            {
                Name = $"Chat with {customer.FullName}",
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow,
                Status = "Open"
            };

            var createdRoom = await _chatRepository.CreateChatRoomAsync(chatRoom);
            _logger.LogInformation("Created new chat room {ChatRoomId} for customer {CustomerId}", createdRoom.Id, customerId);

            return await _chatRepository.GetChatRoomByIdAsync(createdRoom.Id);
        }

        public async Task<IEnumerable<ChatRoom>> GetChatRoomsByStaffIdAsync(int staffId)
        {
            return await _chatRepository.GetChatRoomsByStaffIdAsync(staffId);
        }

        public async Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync()
        {
            return await _chatRepository.GetActiveChatRoomsAsync();
        }

        public async Task<ChatRoom> AssignStaffToChatRoomAsync(int chatRoomId, int staffId)
        {
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(chatRoomId);
            if (chatRoom == null)
            {
                throw new ArgumentException("Chat room not found", nameof(chatRoomId));
            }

            var staff = await _userService.GetUserByIdAsync(staffId);
            if (staff == null || (staff.Role != "Staff" && staff.Role != "Admin"))
            {
                throw new ArgumentException("Invalid staff member", nameof(staffId));
            }

            chatRoom.StaffId = staffId;
            chatRoom.Status = "InProgress";
            await _chatRepository.UpdateChatRoomAsync(chatRoom);

            // Send system message
            await SendSystemMessageAsync(chatRoomId, $"{staff.FullName} has joined the conversation");

            _logger.LogInformation("Assigned staff {StaffId} to chat room {ChatRoomId}", staffId, chatRoomId);
            return chatRoom;
        }

        public async Task<ChatRoom> UpdateChatRoomStatusAsync(int chatRoomId, string status)
        {
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(chatRoomId);
            if (chatRoom == null)
            {
                throw new ArgumentException("Chat room not found", nameof(chatRoomId));
            }

            chatRoom.Status = status;
            await _chatRepository.UpdateChatRoomAsync(chatRoom);

            return chatRoom;
        }

        public async Task CloseChatRoomAsync(int chatRoomId)
        {
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(chatRoomId);
            if (chatRoom == null)
            {
                throw new ArgumentException("Chat room not found", nameof(chatRoomId));
            }

            chatRoom.Status = "Closed";
            chatRoom.IsActive = false;
            await _chatRepository.UpdateChatRoomAsync(chatRoom);

            // Send system message
            await SendSystemMessageAsync(chatRoomId, "This conversation has been closed");

            _logger.LogInformation("Closed chat room {ChatRoomId}", chatRoomId);
        }

        // Message operations
        public async Task<IEnumerable<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId)
        {
            return await _chatRepository.GetMessagesByChatRoomIdAsync(chatRoomId);
        }

        public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int chatRoomId, int count = 50)
        {
            return await _chatRepository.GetRecentMessagesAsync(chatRoomId, count);
        }

        public async Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderId, string content, string messageType = "Text")
        {
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(chatRoomId);
            if (chatRoom == null)
            {
                throw new ArgumentException("Chat room not found", nameof(chatRoomId));
            }

            if (!chatRoom.IsActive)
            {
                throw new InvalidOperationException("Cannot send message to inactive chat room");
            }

            var message = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                Content = content,
                MessageType = messageType,
                SentAt = DateTime.UtcNow
            };

            var createdMessage = await _chatRepository.CreateMessageAsync(message);
            _logger.LogInformation("Message sent in chat room {ChatRoomId} by user {SenderId}", chatRoomId, senderId);

            return createdMessage;
        }

        public async Task<ChatMessage> SendSystemMessageAsync(int chatRoomId, string content)
        {
            var message = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = 0, // System user
                Content = content,
                MessageType = "System",
                SentAt = DateTime.UtcNow,
                IsRead = true
            };

            return await _chatRepository.CreateMessageAsync(message);
        }

        public async Task MarkMessagesAsReadAsync(int chatRoomId, int userId)
        {
            await _chatRepository.MarkMessagesAsReadAsync(chatRoomId, userId);
        }

        public async Task DeleteMessageAsync(int messageId, int userId)
        {
            // Only allow users to delete their own messages
            var message = await _chatRepository.GetMessagesByChatRoomIdAsync(0); // This needs to be fixed
            // For now, just delete the message
            await _chatRepository.DeleteMessageAsync(messageId);
        }

        // User connection management
        public async Task<UserConnection> AddUserConnectionAsync(int userId, string connectionId)
        {
            // Remove any existing connections for this user
            await _chatRepository.DeleteConnectionsByUserIdAsync(userId);

            var connection = new UserConnection
            {
                UserId = userId,
                ConnectionId = connectionId,
                ConnectedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsOnline = true
            };

            return await _chatRepository.CreateConnectionAsync(connection);
        }

        public async Task RemoveUserConnectionAsync(string connectionId)
        {
            await _chatRepository.DeleteConnectionAsync(connectionId);
        }

        public async Task UpdateUserActivityAsync(int userId)
        {
            var connection = await _chatRepository.GetConnectionByUserIdAsync(userId);
            if (connection != null)
            {
                connection.LastActivity = DateTime.UtcNow;
                await _chatRepository.UpdateConnectionAsync(connection);
            }
        }

        public async Task<IEnumerable<User>> GetOnlineUsersAsync()
        {
            return await _chatRepository.GetOnlineUsersAsync();
        }

        public async Task<bool> IsUserOnlineAsync(int userId)
        {
            var connection = await _chatRepository.GetConnectionByUserIdAsync(userId);
            return connection != null && connection.IsOnline;
        }

        // Statistics and notifications
        public async Task<int> GetUnreadMessageCountAsync(int userId)
        {
            return await _chatRepository.GetUnreadMessageCountAsync(userId);
        }

        public async Task<int> GetActiveChatRoomsCountAsync()
        {
            return await _chatRepository.GetActiveChatRoomsCountAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetChatRoomsWithUnreadMessagesAsync(int userId)
        {
            var allRooms = await _chatRepository.GetAllChatRoomsAsync();
            var roomsWithUnread = new List<ChatRoom>();

            foreach (var room in allRooms)
            {
                if ((room.CustomerId == userId || room.StaffId == userId))
                {
                    var unreadCount = await _chatRepository.GetUnreadMessageCountAsync(userId);
                    if (unreadCount > 0)
                    {
                        roomsWithUnread.Add(room);
                    }
                }
            }

            return roomsWithUnread;
        }

        // Staff management
        public async Task<IEnumerable<User>> GetAvailableStaffAsync()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            return allUsers.Where(u => u.Role == "Staff" || u.Role == "Admin").Where(u => u.IsActive);
        }

        public async Task<User?> GetLeastBusyStaffAsync()
        {
            var availableStaff = await GetAvailableStaffAsync();
            
            User? leastBusyStaff = null;
            int minChatRooms = int.MaxValue;

            foreach (var staff in availableStaff)
            {
                var chatRooms = await _chatRepository.GetChatRoomsByStaffIdAsync(staff.Id);
                var activeChatRoomsCount = chatRooms.Count(cr => cr.IsActive);

                if (activeChatRoomsCount < minChatRooms)
                {
                    minChatRooms = activeChatRoomsCount;
                    leastBusyStaff = staff;
                }
            }

            return leastBusyStaff;
        }
    }
}
