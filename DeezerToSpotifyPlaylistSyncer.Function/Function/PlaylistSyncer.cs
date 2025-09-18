using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Services;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DeezerToSpotifyPlaylistSyncer.Function.Function;

public class PlaylistSyncer(ILogger<PlaylistSyncer> logger, IDeezerPlaylistService deezerPlaylistService, ISpotifyPlaylistService spotifyPlaylistService)
{
	private readonly ILogger<PlaylistSyncer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	private readonly IDeezerPlaylistService _deezerPlaylistService = deezerPlaylistService ?? throw new ArgumentNullException(nameof(deezerPlaylistService));
	private readonly ISpotifyPlaylistService _spotifyPlaylistService = spotifyPlaylistService ?? throw new ArgumentNullException(nameof(spotifyPlaylistService));

	[Function(nameof(PlaylistSyncer))]
	public async Task RunAsync([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer)
	{
		this._logger.LogWarning("Playlist syncer started");
		var deezerPlaylist = await this._deezerPlaylistService.GetPlaylistAsync();
		if (deezerPlaylist?.Tracks is null)
		{
			this._logger.LogError("Fatal error: deezer playlist not found");
			return;
		}

		var detailedDeezerTracks = await this._deezerPlaylistService.GetDetailedTracksAsync(deezerPlaylist.Tracks.Data.Select(data => data.Id));
		var spotifyTracks = await this._spotifyPlaylistService.GetTrackIdsAsync(detailedDeezerTracks);
		var spotifyPlaylist = await this._spotifyPlaylistService.GetPlaylistAsync();
		if (spotifyPlaylist is null)
		{
			this._logger.LogError("Fatal error: spotify playlist not found");
			return;
		}

		await this._spotifyPlaylistService.AddMissingTracksAsync(spotifyPlaylist, spotifyTracks);
		await this._spotifyPlaylistService.RemoveOldTracksAsync(spotifyPlaylist, spotifyTracks);

		this._logger.LogWarning("Playlist syncer ended");
	}
}
