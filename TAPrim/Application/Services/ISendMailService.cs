namespace TAPrim.Application.Services
{
	public interface ISendMailService
	{
		Task SendMailByMailTemplateIdAsync(string mailTemplateId, string email, dynamic data);

	}
}
