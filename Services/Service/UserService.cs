using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;
        private readonly IUserRepository _repository;
        private readonly IEmailService _emailService;

        public UserService(ApplicationDbContext context, JwtService jwt, IUserRepository repository, IEmailService emailService)
        {
            _context = context;
            _jwt = jwt;
            _repository = repository;
            _emailService = emailService;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task UpdateUserAsync(User updatedUser)
        {
            var existingUser = await _context.Users.FindAsync(updatedUser.Id);
            if (existingUser == null) return;

            existingUser.FullName = updatedUser.FullName;
            existingUser.Email = updatedUser.Email;
            existingUser.Role = updatedUser.Role;
            existingUser.PasswordHash = string.IsNullOrWhiteSpace(updatedUser.PasswordHash)
                ? existingUser.PasswordHash
                : updatedUser.PasswordHash;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Đăng ký người dùng bằng email, không cần mật khẩu
        /// </summary>
        public async Task<bool> RegisterAsync(UserRegisterRequest request)
        {
            var user = await _repository.GetByEmailAsync(request.Email);
            if (user != null) return false;

            var newUser = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = "User",
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendVerificationCodeAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Tạo user mới nếu chưa tồn tại
                user = new User
                {
                    Email = email,
                    Role = "User",
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Tạo mã OTP
            var code = new Random().Next(100000, 999999).ToString();

            // Xóa mã cũ nếu có
            var existingCode = await _context.VerificationCodes.FirstOrDefaultAsync(v => v.Email == email);
            if (existingCode != null)
            {
                _context.VerificationCodes.Remove(existingCode);
            }

            // Lưu mã mới
            var verification = new VerificationCode
            {
                Email = email,
                Code = code,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5)
            };
            _context.VerificationCodes.Add(verification);
            await _context.SaveChangesAsync();

            // Gửi mã OTP qua email
            await _emailService.SendEmailAsync(email, "Your Login Code", $"Your verification code is: <b>{code}</b>");
            return true;
        }

        public async Task<LoginResponse?> VerifyCodeAndLoginAsync(string email, string code)
        {
            var record = await _context.VerificationCodes
                .Where(v => v.Email == email && v.Code == code)
                .OrderByDescending(v => v.ExpiredAt)
                .FirstOrDefaultAsync();

            if (record == null || record.ExpiredAt < DateTime.UtcNow)
            {
                return null; // Mã không hợp lệ hoặc hết hạn
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            // Tạo token JWT nếu có hệ thống auth
            var token = _jwt.GenerateToken(user);

            // Xóa mã đã dùng
            _context.VerificationCodes.Remove(record);
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role
            };
        }

    }
}
