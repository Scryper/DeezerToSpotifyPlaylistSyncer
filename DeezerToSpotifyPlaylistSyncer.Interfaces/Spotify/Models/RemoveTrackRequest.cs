using System.Text.Json.Serialization;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;

public class RemoveTrackRequest
{
	[JsonPropertyName("tracks")]
	public required IEnumerable<Track> Tracks { get; set; }
}

public class Track
{
	[JsonPropertyName("uri")]
	public required string Uri { get; set; }
}
