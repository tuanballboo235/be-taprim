using TAPrim.Models;

namespace TAPrim.Application.Services
{
	public interface IJwtSService
	{
		public string GenerateToken(User account);
	}
}
