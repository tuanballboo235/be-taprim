using System.Net.Mail;
using System.Net;

namespace BasketballAcademyManagementSystemAPI.Common.Helpers
{
    public class EmailHelper
    {
    	private readonly IConfiguration _configuration;

	private GmailSMTPSetting gmailSMTPSettings;
	private static readonly string SmtpServer = "smtp.gmail.com";
	private static readonly int SmtpPort = 587;
	
	public EmailHelper(IConfiguration configuration)
	{
	    _configuration = configuration;
	    GetGmailSMTPSettings();
	}
	
	public void GetGmailSMTPSettings()
	{
	    var gmailSMTPSettingsSection = _configuration.GetSection("GmailSMTPSettings");
	    gmailSMTPSettings = gmailSMTPSettingsSection.Get<GmailSMTPSetting>();
	}

        public void SendEmailMultiThread(string email, string subject, string body)
        {
            SendEmailAsync(email, subject, body).Wait();
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(GmailSMTPSetting.SenderEmail, GmailSMTPSetting.SenderPassword)
            };
            var senderAddress = new MailAddress(GmailSMTPSetting.SenderEmail, GmailSMTPSetting.SenderName);
            var mailMessage = new MailMessage
            {
                From = senderAddress,
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }

        public void SendEmailMultiThread(string subject, string body, List<string> ccEmails)
        {
            SendEmailAsync(subject, body, ccEmails).Wait();
        }

        public Task SendEmailAsync(string subject, string body, List<string> ccEmails)
        {
            var client = new SmtpClient(SmtpServer, SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(GmailSMTPSetting.SenderEmail, GmailSMTPSetting.SenderPassword)
            };

            var senderAddress = new MailAddress(GmailSMTPSetting.SenderEmail, GmailSMTPSetting.SenderName);
            var mailMessage = new MailMessage
            {
                From = senderAddress,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            if (ccEmails != null)
            {
                foreach (var ccEmail in ccEmails)
                {
                    if (!string.IsNullOrWhiteSpace(ccEmail))
                    {
                        mailMessage.CC.Add(ccEmail);
                    }
                }
            }

            return client.SendMailAsync(mailMessage);
        }

        //Bổ sung method sendEmail return boolean
        public static async Task<bool> SendEmailBoolAsync(string email, string subject, string message)
		{
			try
			{

				var client = new SmtpClient("smtp.gmail.com", 587)
				{
					EnableSsl = true,
					Credentials = new NetworkCredential(GmailSMTPSetting.SenderEmail, GmailSMTPSetting.SenderPassword)
				};
				var senderAddress = new MailAddress(GmailSMTPSetting.SenderEmail, GmailSMTPSetting.SenderName);

				var mailMessage = new MailMessage
				{
					From = senderAddress,
					Subject = subject,
					Body = message,
					IsBodyHtml = true
				};

				mailMessage.To.Add(email);
				await client.SendMailAsync(mailMessage);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error sending email: {ex.Message}");
				return false;
			}
		}

		public static async Task SendEmailMultiThreadAsync(string email, string subject, string body)
		{
			await Task.Run(() => SendEmailBoolAsync(email, subject, body));
		}

	}
}
