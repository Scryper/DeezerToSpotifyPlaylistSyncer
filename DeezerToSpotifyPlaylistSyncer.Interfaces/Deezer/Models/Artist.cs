using System.Text.Json.Serialization;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;

public class Artist
{
	[JsonPropertyName("name")]
	public required string Name { get; set; }
}
