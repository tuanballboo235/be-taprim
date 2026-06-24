using TAPrim.Models;

namespace TAPrim.Application.Services
{
	public interface IAuthService
	{
		Task<User?> LoginAsync(string usernameOrEmail, string password);

		Task<User?> GetByIdAsync(int userId);
	}
}
