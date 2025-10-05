namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Mails.Configuration;

public class MailSettings
{
	public required string ApiKey { get; set; }
	public required string To { get; set; }
	public required long TemplateId { get; set; }
}