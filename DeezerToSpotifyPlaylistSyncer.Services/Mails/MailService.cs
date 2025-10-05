using brevo_csharp.Api;
using brevo_csharp.Model;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Mails.Configuration;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Mails.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Configuration = brevo_csharp.Client.Configuration;

namespace DeezerToSpotifyPlaylistSyncer.Services.Mails;

public class MailService(
	IOptions<MailSettings> mailSettings,
	ILogger<MailService> logger)
	: IMailService
{
	private readonly MailSettings _mailSettings = mailSettings?.Value ?? throw new ArgumentNullException(nameof(mailSettings));
	private readonly ILogger<MailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	public async Task<bool> SendMailAsync(object addedSong)
	{
		Configuration.Default.AddApiKey("api-key", this._mailSettings.ApiKey);
		var client = new TransactionalEmailsApi();
		var mail = new SendSmtpEmail
		{
			To = [new SendSmtpEmailTo(this._mailSettings.To, "Mon gaté")],
			TemplateId = this._mailSettings.TemplateId,
			Params = addedSong
		};

		try
		{
			await client.SendTransacEmailAsync(mail);
		}
		catch (Exception e)
		{
			this._logger.LogError("An error occured when sending mail : {Message}", e.Message);
			return false;
		}

		return true;
	}
}