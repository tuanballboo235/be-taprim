using Newtonsoft.Json;

namespace TAPrim.Application.DTOs.Tempmail
{
	public class TempmailApiResponseDto<T>
	{
		[JsonProperty("success")]
		public bool Success { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
		[JsonProperty("data")]
		public T Data { get; set; }
	}
}
