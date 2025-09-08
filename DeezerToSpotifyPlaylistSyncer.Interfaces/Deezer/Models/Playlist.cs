using System.Text.Json.Serialization;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Models;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;

public class Playlist : IPlaylist
{
	[JsonPropertyName("tracks")]
	public Tracks? Tracks { get; set; }
}
