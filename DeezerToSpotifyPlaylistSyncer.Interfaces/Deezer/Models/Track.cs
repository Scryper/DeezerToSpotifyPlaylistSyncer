using System.Text.Json.Serialization;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;

public class Track
{
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	[JsonPropertyName("artist")]
	public required Artist Artist { get; set; }
}