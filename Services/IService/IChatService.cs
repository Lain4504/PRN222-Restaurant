using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services.IService
{
    public interface IChatService
    {
        // ChatRoom operations
        Task<IEnumerable<ChatRoom>> GetAllChatRoomsAsync();
        Task<ChatRoom?> GetChatRoomByIdAsync(int id);
        Task<ChatRoom?> GetOrCreateChatRoomAsync(int customerId);
        Task<IEnumerable<ChatRoom>> GetChatRoomsByStaffIdAsync(int staffId);
        Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync();
        Task<ChatRoom> AssignStaffToChatRoomAsync(int chatRoomId, int staffId);
        Task<ChatRoom> UpdateChatRoomStatusAsync(int chatRoomId, string status);
        Task CloseChatRoomAsync(int chatRoomId);

        // Message operations
        Task<IEnumerable<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId);
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int chatRoomId, int count = 50);
        Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderId, string content, string messageType = "Text");
        Task<ChatMessage> SendSystemMessageAsync(int chatRoomId, string content);
        Task MarkMessagesAsReadAsync(int chatRoomId, int userId);
        Task DeleteMessageAsync(int messageId, int userId);

        // User connection management
        Task<UserConnection> AddUserConnectionAsync(int userId, string connectionId);
        Task RemoveUserConnectionAsync(string connectionId);
        Task UpdateUserActivityAsync(int userId);
        Task<IEnumerable<User>> GetOnlineUsersAsync();
        Task<bool> IsUserOnlineAsync(int userId);

        // Statistics and notifications
        Task<int> GetUnreadMessageCountAsync(int userId);
        Task<int> GetActiveChatRoomsCountAsync();
        Task<IEnumerable<ChatRoom>> GetChatRoomsWithUnreadMessagesAsync(int userId);

        // Staff management
        Task<IEnumerable<User>> GetAvailableStaffAsync();
        Task<User?> GetLeastBusyStaffAsync();
    }
}
