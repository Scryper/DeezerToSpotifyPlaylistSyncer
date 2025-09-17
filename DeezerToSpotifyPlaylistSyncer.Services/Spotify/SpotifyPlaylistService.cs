using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Configuration;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.CompilerServices;

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

		var playlist = await this._httpClient.GetFromJsonAsync<SpotifyPlaylist>($"playlists/{this._spotifyConfiguration.PlaylistId}");
		if (!string.IsNullOrWhiteSpace(playlist?.Tracks?.Next))
		{
			var next = playlist.Tracks.Next;
			do
			{
				var response = await this._httpClient.GetFromJsonAsync<SpotifyTracks>(next);
				next = response?.Next;
				if (response is not null)
				{
					playlist?.Tracks?.Items.AddRange(response.Items);
				}
			} while (!string.IsNullOrWhiteSpace(next));
		}

		return playlist;
	}

	public async Task<IList<SpotifyTrack>> GetTrackIdsAsync(IEnumerable<DeezerTrack> detailedDeezerTracks)
	{
		var ids = new List<SpotifyTrack>();
		foreach (var deezerTrack in detailedDeezerTracks.Where(deezerTrack => deezerTrack.Contributors is not null))
		{
			var trackResponse = await this.SearchTrackAsync(deezerTrack.Contributors!.Select(contributor => contributor.Name).ToList(), deezerTrack.Title);
			if (trackResponse?.Tracks is not null)
			{
				ids.Add(trackResponse.Tracks.Items.First());
			}
		}

		return ids;
	}

	public async Task AddMissingTracksAsync(SpotifyPlaylist spotifyPlaylist, IList<SpotifyTrack> spotifyTracks)
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

		var missingTracks = spotifyTracks.Where(track => !spotifyPlaylist.Tracks.Items.Select(item => item.Track?.Id).Contains(track.Id));
		var batches = missingTracks
			.Select((item, index) => new { item, index })
			.GroupBy(item => item.index / 100)
			.Select(group => group.Select(x => x.item).ToList());

		foreach (var batch in batches)
		{
			this._logger.LogWarning("Adding tracks to spotify playlist : {Tracks}", string.Join(", ", batch.Select(track => track.Name)));
			var response = await this._httpClient.PostAsJsonAsync(
				$"playlists/{this._spotifyConfiguration.PlaylistId}/tracks",
				new AddTrackRequest { Uris = batch.Select(track => $"spotify:track:{track.Id}") });
			response.EnsureSuccessStatusCode();
		}
	}

	public async Task RemoveOldTracksAsync(SpotifyPlaylist spotifyPlaylist, IList<SpotifyTrack> spotifyTracks)
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

		var tracksToRemove = spotifyPlaylist.Tracks.Items.Where(track => !spotifyTracks.Select(sTrack => sTrack.Id).Contains(track.Track?.Id));
		var batches = tracksToRemove
			.Select((item, index) => new { item, index })
			.GroupBy(item => item.index / 100)
			.Select(group => group.Select(x => x.item).ToList());

		foreach (var batch in batches)
		{
			this._logger.LogWarning("Deleting tracks from spotify playlist : {Tracks}", string.Join(", ", batch.Select(track => track.Track?.Name)));
			var request = new HttpRequestMessage(HttpMethod.Delete, $"playlists/{this._spotifyConfiguration.PlaylistId}/tracks")
			{
				Content = new StringContent(JsonSerializer.Serialize(
					new RemoveTrackRequest { Tracks = batch.Where(track => !string.IsNullOrWhiteSpace(track.Track?.Id)).Select(track => new Track { Uri = $"spotify:track:{track.Track.Id}" }) }),
					Encoding.UTF8,
					MediaTypeNames.Application.Json)
			};

			var response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}

	private async Task<SpotifyPlaylist?> SearchTrackAsync(IList<string> artists, string track)
	{
		var artistsQuery = string.Join(" ", artists.Select(artist => $"artist:{artist}"));
		var response = await this._httpClient.GetAsync($"search?q={Uri.EscapeDataString($"{artistsQuery} track:{track}")}&type=track&limit=1");
		if (response.StatusCode == HttpStatusCode.TooManyRequests)
		{
			var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 1;
			await Task.Delay(TimeSpan.FromSeconds(retryAfter));
			return await this.SearchTrackAsync(artists, track);
		}

		return JsonSerializer.Deserialize<SpotifyPlaylist>(await response.Content.ReadAsStringAsync(), this._jsonOptions);
	}
}
