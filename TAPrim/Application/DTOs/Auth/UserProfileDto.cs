namespace TAPrim.Application.DTOs.Auth
{
	public class UserProfileDto
	{
		public int Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string? Email { get; set; }
		public string? Phone { get; set; }
		public string Role { get; set; } = string.Empty;
	}
}