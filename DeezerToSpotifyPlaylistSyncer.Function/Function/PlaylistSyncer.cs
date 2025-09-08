using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DeezerToSpotifyPlaylistSyncer.Function.Function;

public class PlaylistSyncer(ILogger<PlaylistSyncer> logger, IDeezerPlaylistService deezerPlaylistService)
{
	private readonly ILogger<PlaylistSyncer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	private readonly IDeezerPlaylistService _deezerPlaylistService = deezerPlaylistService ?? throw new ArgumentNullException(nameof(deezerPlaylistService));

	[Function(nameof(PlaylistSyncer))]
	public async Task RunAsync([TimerTrigger("0 48 20 * * *")] TimerInfo myTimer)
	{
		this._logger.LogInformation("Playlist syncer started");
		var deezerPlaylist = await this._deezerPlaylistService.GetPlaylistAsync();

		this._logger.LogInformation("Playlist syncer ended");
	}
}