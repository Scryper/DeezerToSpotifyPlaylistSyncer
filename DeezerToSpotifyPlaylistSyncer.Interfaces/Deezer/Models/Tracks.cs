using System.Text.Json.Serialization;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;

public class Tracks
{
	[JsonPropertyName("data")]
	public IEnumerable<Track> TracksData { get; set; } = [];
}