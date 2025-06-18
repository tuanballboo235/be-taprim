using Microsoft.EntityFrameworkCore;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
	public class SendMailRepository:ISendMailRepository
	{
		private readonly TaprimContext _context;
		public SendMailRepository(TaprimContext taprimContext) { 
		
			_context = taprimContext;
		}

		public async Task<MailTemplate?> GetByIdAsync(string id)
		{
			return await _context.MailTemplates.FindAsync(id);
		}
	}
}
