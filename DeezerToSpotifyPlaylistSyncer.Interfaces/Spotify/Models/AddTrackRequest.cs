using System.Text.Json.Serialization;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;

public class AddTrackRequest
{
	[JsonPropertyName("uris")]
	public required IEnumerable<string> Uris { get; set; }

	[JsonPropertyName("position")]
	public int Position = 0;
}
