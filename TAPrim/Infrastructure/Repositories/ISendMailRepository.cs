using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
	public interface ISendMailRepository
	{
		Task<MailTemplate?> GetByIdAsync(string id);

	}
}
