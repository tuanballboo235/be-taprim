using Newtonsoft.Json;

namespace TAPrim.Application.DTOs.Tempmail
{
	public class TempmailDataDto
	{
		[JsonProperty("items")]
		public List<TempmailEmailItemDto> Items { get; set; }
		[JsonProperty("pagination")]
		public TempmailPaginationDto Pagination { get; set; }
	}
}
