using System.Text.Json;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Services;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Mails.Services;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DeezerToSpotifyPlaylistSyncer.Function.Function;

public class PlaylistSyncer(
	ILogger<PlaylistSyncer> logger,
	IDeezerPlaylistService deezerPlaylistService,
	ISpotifyPlaylistService spotifyPlaylistService,
	IMailService mailService)
{
	private readonly ILogger<PlaylistSyncer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	private readonly IDeezerPlaylistService _deezerPlaylistService = deezerPlaylistService ?? throw new ArgumentNullException(nameof(deezerPlaylistService));
	private readonly ISpotifyPlaylistService _spotifyPlaylistService = spotifyPlaylistService ?? throw new ArgumentNullException(nameof(spotifyPlaylistService));
	private readonly IMailService _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));

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

		var addedTracks = await this._spotifyPlaylistService.AddMissingTracksAsync(spotifyPlaylist, spotifyTracks);
		var removedTracks = await this._spotifyPlaylistService.RemoveOldTracksAsync(spotifyPlaylist, spotifyTracks);

		this._logger.LogWarning(JsonSerializer.Serialize(addedTracks));
		this._logger.LogWarning(JsonSerializer.Serialize(removedTracks));
		if (addedTracks is not null && removedTracks is not null)
		{
			addedTracks.ToList().ForEach(t => this._logger.LogWarning("Added {Name}", t.Name));
			removedTracks.ToList().ForEach(t => this._logger.LogWarning("Removed {Name}", t.Track?.Name));
			var firstAddedTrack = addedTracks.ExceptBy(removedTracks.Select(track => track.Track?.Name), track => track.Name).FirstOrDefault();
			addedTracks.ExceptBy(removedTracks.Select(track => track.Track?.Name), track => track.Name).ToList().ForEach(t => this._logger.LogWarning("Track left {Name}", t.Name));
			this._logger.LogWarning("First track {Value}", JsonSerializer.Serialize(firstAddedTrack));
			this._logger.LogWarning("First track {Name}", firstAddedTrack?.Name);
			if (firstAddedTrack is not null)
			{
				/*var mailResult = await this._mailService.SendMailAsync(new
				{
					Artist = firstAddedTrack.Artists?.First().Name,
					Title = firstAddedTrack.Name
				});
				if (mailResult)
				{
					this._logger.LogWarning("Mail sent successfully");
				}
				else
				{
					this._logger.LogError("Fatal error: an error occured while sending mail");
				}*/
			}
		}

		this._logger.LogWarning("Playlist syncer ended");
	}
}
