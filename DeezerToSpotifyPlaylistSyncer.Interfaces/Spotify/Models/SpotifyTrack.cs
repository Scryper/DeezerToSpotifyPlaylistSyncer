namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;

public class SpotifyTrack
{
	public string Id { get; set; } = string.Empty;
	public required string Name { get; set; }
	public required IEnumerable<SpotifyArtist> Artists { get; set; }
}
