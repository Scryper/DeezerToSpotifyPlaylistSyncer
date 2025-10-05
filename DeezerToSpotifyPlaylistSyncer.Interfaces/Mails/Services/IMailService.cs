namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Mails.Services;

public interface IMailService
{
	Task<bool> SendMailAsync(object addedSongs);
}
