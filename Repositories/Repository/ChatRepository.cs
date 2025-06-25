using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;

namespace PRN222_Restaurant.Repositories.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ChatRoom methods
        public async Task<IEnumerable<ChatRoom>> GetAllChatRoomsAsync()
        {
            return await _context.ChatRooms
                .Include(cr => cr.Customer)
                .Include(cr => cr.Staff)
                .Include(cr => cr.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .OrderByDescending(cr => cr.LastMessageAt)
                .ToListAsync();
        }

        public async Task<List<ChatRoom>> GetAllChatRoomsForAdminAsync()
        {
            return await _context.ChatRooms
                .Include(cr => cr.Customer)
                .Include(cr => cr.Staff)
                .Include(cr => cr.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.Sender)
                .Where(cr => cr.IsActive)
                .OrderByDescending(cr => cr.LastMessageAt)
                .ToListAsync();
        }

        public async Task<ChatRoom?> GetChatRoomByIdAsync(int id)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Customer)
                .Include(cr => cr.Staff)
                .Include(cr => cr.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(cr => cr.Id == id);
        }

        public async Task<ChatRoom?> GetChatRoomByCustomerIdAsync(int customerId)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Customer)
                .Include(cr => cr.Staff)
                .Include(cr => cr.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(cr => cr.CustomerId == customerId && cr.IsActive);
        }

        public async Task<IEnumerable<ChatRoom>> GetChatRoomsByStaffIdAsync(int staffId)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Customer)
                .Include(cr => cr.Staff)
                .Include(cr => cr.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(cr => cr.StaffId == staffId && cr.IsActive)
                .OrderByDescending(cr => cr.LastMessageAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync()
        {
            return await _context.ChatRooms
                .Include(cr => cr.Customer)
                .Include(cr => cr.Staff)
                .Include(cr => cr.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(cr => cr.IsActive)
                .OrderByDescending(cr => cr.LastMessageAt)
                .ToListAsync();
        }

        public async Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom)
        {
            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();
            return chatRoom;
        }

        public async Task UpdateChatRoomAsync(ChatRoom chatRoom)
        {
            _context.ChatRooms.Update(chatRoom);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteChatRoomAsync(int id)
        {
            var chatRoom = await _context.ChatRooms.FindAsync(id);
            if (chatRoom != null)
            {
                _context.ChatRooms.Remove(chatRoom);
                await _context.SaveChangesAsync();
            }
        }

        // ChatMessage methods
        public async Task<IEnumerable<ChatMessage>> GetMessagesByChatRoomIdAsync(int chatRoomId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int chatRoomId, int count = 50)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> CreateMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            
            // Update last message time in chat room
            var chatRoom = await _context.ChatRooms.FindAsync(message.ChatRoomId);
            if (chatRoom != null)
            {
                chatRoom.LastMessageAt = message.SentAt;
                await _context.SaveChangesAsync();
            }
            
            return message;
        }

        public async Task UpdateMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMessageAsync(int id)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            if (message != null)
            {
                message.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkMessagesAsReadAsync(int chatRoomId, int userId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        // UserConnection methods
        public async Task<IEnumerable<UserConnection>> GetActiveConnectionsAsync()
        {
            return await _context.UserConnections
                .Include(uc => uc.User)
                .Where(uc => uc.IsOnline)
                .ToListAsync();
        }

        public async Task<UserConnection?> GetConnectionByUserIdAsync(int userId)
        {
            return await _context.UserConnections
                .Include(uc => uc.User)
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.IsOnline);
        }

        public async Task<UserConnection?> GetConnectionByConnectionIdAsync(string connectionId)
        {
            return await _context.UserConnections
                .Include(uc => uc.User)
                .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId);
        }

        public async Task<UserConnection> CreateConnectionAsync(UserConnection connection)
        {
            _context.UserConnections.Add(connection);
            await _context.SaveChangesAsync();
            return connection;
        }

        public async Task UpdateConnectionAsync(UserConnection connection)
        {
            _context.UserConnections.Update(connection);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteConnectionAsync(string connectionId)
        {
            var connection = await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId);
            
            if (connection != null)
            {
                _context.UserConnections.Remove(connection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteConnectionsByUserIdAsync(int userId)
        {
            var connections = await _context.UserConnections
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            _context.UserConnections.RemoveRange(connections);
            await _context.SaveChangesAsync();
        }

        // Statistics
        public async Task<int> GetUnreadMessageCountAsync(int userId)
        {
            return await _context.ChatMessages
                .Where(m => m.SenderId != userId && !m.IsRead && 
                           _context.ChatRooms.Any(cr => cr.Id == m.ChatRoomId && 
                                                       (cr.CustomerId == userId || cr.StaffId == userId)))
                .CountAsync();
        }

        public async Task<int> GetActiveChatRoomsCountAsync()
        {
            return await _context.ChatRooms
                .Where(cr => cr.IsActive)
                .CountAsync();
        }

        public async Task<IEnumerable<User>> GetOnlineUsersAsync()
        {
            return await _context.UserConnections
                .Include(uc => uc.User)
                .Where(uc => uc.IsOnline)
                .Select(uc => uc.User)
                .Distinct()
                .ToListAsync();
        }
    }
}
