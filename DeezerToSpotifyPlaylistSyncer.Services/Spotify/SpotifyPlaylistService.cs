using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Configuration;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeezerToSpotifyPlaylistSyncer.Services.Spotify;

public class SpotifyPlaylistService(
	HttpClient httpClient, 
	IOptions<SpotifyConfiguration> spotifyConfiguration,
	ILogger<SpotifyPlaylistService> logger) : ISpotifyPlaylistService
{
	private readonly ILogger<SpotifyPlaylistService> _logger = logger ?? throw new NullReferenceException(nameof(logger));

	private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	private readonly SpotifyConfiguration _spotifyConfiguration = spotifyConfiguration.Value ?? throw new ArgumentNullException(nameof(spotifyConfiguration));

	private readonly JsonSerializerOptions _jsonOptions = new () { PropertyNameCaseInsensitive = true };

	public async Task<SpotifyPlaylist?> GetPlaylistAsync()
	{
		if (string.IsNullOrWhiteSpace(this._spotifyConfiguration.PlaylistId))
		{
			this._logger.LogError("Fatal error : playlist id not set");
			return null;
		}

		return await this._httpClient.GetFromJsonAsync<SpotifyPlaylist>($"playlists/{this._spotifyConfiguration.PlaylistId}");
	}

	public async Task<IList<string>> GetTrackIdsAsync(DeezerPlaylist deezerPlaylist)
	{
		if (deezerPlaylist.Tracks is null)
		{
			this._logger.LogError("Fatal error : deezer tracks not set");
			return [];
		}

		return await this.SearchTrackIdsAsync(deezerPlaylist.Tracks.Data);
	}

	public async Task AddMissingTracksAsync(SpotifyPlaylist spotifyPlaylist, IList<string> spotifyTrackIds)
	{
		if (string.IsNullOrWhiteSpace(this._spotifyConfiguration.PlaylistId))
		{
			this._logger.LogError("Fatal error : playlist id not set");
			return;
		}

		if (spotifyPlaylist.Tracks is null)
		{
			this._logger.LogError("Fatal error : there are no tracks in spotify playlist");
			return;
		}

		var missingTracks = spotifyTrackIds.Except(spotifyPlaylist.Tracks.Items.Select(item => item.Id)).ToList();
		var batches = missingTracks
			.Select((item, index) => new { item, index })
			.GroupBy(item => item.index / 100)
			.Select(group => group.Select(x => x.item).ToList());

		foreach (var batch in batches)
		{
			var response = await this._httpClient.PostAsync(
				$"playlists/{this._spotifyConfiguration.PlaylistId}/tracks",
				new StringContent(JsonSerializer.Serialize(new AddTrackRequest { Uris = batch.Select(track => $"spotify:track:{track}") })));
			response.EnsureSuccessStatusCode();
		}
	}

	private async Task<IList<string>> SearchTrackIdsAsync(IEnumerable<DeezerTrack> deezerTracks)
	{
		var ids = new List<string>();
		foreach (var deezerTrack in deezerTracks)
		{
			var trackResponse = await this.SearchTrackAsync(deezerTrack.Artist.Name, deezerTrack.Title);
			if (trackResponse?.Tracks is not null)
			{
				ids.Add(trackResponse.Tracks.Items.First().Id);
			}
		}

		return ids;
	}

	private async Task<SpotifyPlaylist?> SearchTrackAsync(string artist, string track)
	{
		var response = await this._httpClient.GetAsync($"search?q={Uri.EscapeDataString($"artist:{artist} track:{track}")}&type=track&limit=1");
		if (response.StatusCode == HttpStatusCode.TooManyRequests)
		{
			var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 1;
			await Task.Delay(TimeSpan.FromSeconds(retryAfter));
			return await this.SearchTrackAsync(artist, track);
		}

		return JsonSerializer.Deserialize<SpotifyPlaylist>(await response.Content.ReadAsStringAsync(), this._jsonOptions);
	}
}
