using System.Net.Http.Json;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Configuration;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeezerToSpotifyPlaylistSyncer.Services.Deezer;

public class DeezerPlaylistService(
	HttpClient httpClient, 
	IOptions<DeezerConfiguration> deezerConfiguration,
	ILogger<DeezerPlaylistService> logger) : IDeezerPlaylistService
{
	private readonly ILogger<DeezerPlaylistService> _logger = logger ?? throw new NullReferenceException(nameof(logger));

	private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	private readonly DeezerConfiguration _deezerConfiguration = deezerConfiguration.Value ?? throw new ArgumentNullException(nameof(deezerConfiguration));

	public async Task<DeezerPlaylist?> GetPlaylistAsync()
	{
		if (string.IsNullOrWhiteSpace(this._deezerConfiguration.PlaylistId))
		{
			this._logger.LogError("Fatal error : playlist id not set");
			return null;
		}

		return await this._httpClient.GetFromJsonAsync<DeezerPlaylist>($"playlist/{this._deezerConfiguration.PlaylistId}");
	}

	public async Task<IEnumerable<DeezerTrack>> GetDetailedTracksAsync(IEnumerable<long> ids)
	{
		var detailedTracks = new List<DeezerTrack>();
		foreach (var id in ids)
		{
			var detailedTrack = await this._httpClient.GetFromJsonAsync<DeezerTrack>($"track/{id}");
			if (detailedTrack is not null)
			{
				detailedTracks.Add(detailedTrack);
			}
		}

		return detailedTracks;
	}
}
