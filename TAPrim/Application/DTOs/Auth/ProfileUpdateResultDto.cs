namespace TAPrim.Application.DTOs.Auth
{
	public class ProfileUpdateResultDto
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public UserProfileDto? User { get; set; }

		public static ProfileUpdateResultDto Failed(string message)
		{
			return new ProfileUpdateResultDto
			{
				Success = false,
				Message = message
			};
		}

		public static ProfileUpdateResultDto Ok(string message, UserProfileDto user)
		{
			return new ProfileUpdateResultDto
			{
				Success = true,
				Message = message,
				User = user
			};
		}
	}
}