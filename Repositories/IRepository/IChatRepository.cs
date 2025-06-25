using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Repositories.IRepository
{
    public interface IChatRepository
    {
        // ChatRoom methods
        Task<IEnumerable<ChatRoom>> GetAllChatRoomsAsync();
        Task<List<ChatRoom>> GetAllChatRoomsForAdminAsync();
        Task<ChatRoom?> GetChatRoomByIdAsync(int id);
        Task<ChatRoom?> GetChatRoomByCustomerIdAsync(int customerId);
        Task<IEnumerable<ChatRoom>> GetChatRoomsByStaffIdAsync(int staffId);
        Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync();
        Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom);
        Task UpdateChatRoomAsync(ChatRoom chatRoom);
        Task DeleteChatRoomAsync(int id);

        // ChatMessage methods
        Task<IEnumerable<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId);
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int chatRoomId, int count = 50);
        Task<ChatMessage> CreateMessageAsync(ChatMessage message);
        Task UpdateMessageAsync(ChatMessage message);
        Task DeleteMessageAsync(int id);
        Task MarkMessagesAsReadAsync(int chatRoomId, int userId);

        // UserConnection methods
        Task<IEnumerable<UserConnection>> GetActiveConnectionsAsync();
        Task<UserConnection?> GetConnectionByUserIdAsync(int userId);
        Task<UserConnection?> GetConnectionByConnectionIdAsync(string connectionId);
        Task<UserConnection> CreateConnectionAsync(UserConnection connection);
        Task UpdateConnectionAsync(UserConnection connection);
        Task DeleteConnectionAsync(string connectionId);
        Task DeleteConnectionsByUserIdAsync(int userId);

        // Statistics
        Task<int> GetUnreadMessageCountAsync(int userId);
        Task<int> GetActiveChatRoomsCountAsync();
        Task<IEnumerable<User>> GetOnlineUsersAsync();
    }
}
