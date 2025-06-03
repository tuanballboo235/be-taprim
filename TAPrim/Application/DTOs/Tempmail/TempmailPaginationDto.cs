using Newtonsoft.Json;

namespace TAPrim.Application.DTOs.Tempmail
{
	public class TempmailPaginationDto
	{
		[JsonProperty("total")]
		public int Total { get; set; }
		[JsonProperty("per_page")]
		public int PerPage { get; set; }
		[JsonProperty("current_page")]
		public int CurrentPage { get; set; }
		[JsonProperty("last_page")]
		public int LastPage { get; set; }
	}
}
