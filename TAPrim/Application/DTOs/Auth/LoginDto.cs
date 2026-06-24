namespace TAPrim.Application.DTOs.Auth
{
	public class LoginDto
	{
		public string? Username { get; set; }

		public string? Email { get; set; }

		public string Password { get; set; } = string.Empty;
	}
}
