using System.Text.Json.Serialization;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Configuration;

public class SpotifyCredentials
{
	[JsonPropertyName("access_token")]
	public required string AccessToken { get; set; }
}
