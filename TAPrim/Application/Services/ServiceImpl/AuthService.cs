using Microsoft.EntityFrameworkCore;
using TAPrim.Application.Services;
using TAPrim.Models;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class AuthService : IAuthService
	{
		private readonly TaprimContext _context;

		public AuthService(TaprimContext context)
		{
			_context = context;
		}

		public async Task<User?> LoginAsync(string usernameOrEmail, string password)
		{
			var loginName = usernameOrEmail.Trim();

			if (string.IsNullOrWhiteSpace(loginName) || string.IsNullOrEmpty(password))
			{
				return null;
			}

			var user = await _context.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Username == loginName && u.IsEnable);

			if (user == null || !string.Equals(user.Password, password, StringComparison.Ordinal))
			{
				return null;
			}

			return user;
		}

		public Task<User?> GetByIdAsync(int userId)
		{
			return _context.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.UserId == userId && u.IsEnable);
		}

		public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
		{
			if (userId <= 0 || string.IsNullOrEmpty(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
			{
				return false;
			}

			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.UserId == userId && u.IsEnable);

			if (user == null || !string.Equals(user.Password, currentPassword, StringComparison.Ordinal))
			{
				return false;
			}

			user.Password = newPassword;
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
