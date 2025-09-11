namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;

public class SpotifyTrackItem
{
	public required string Id { get; set; }
	public required string Name { get; set; }
	public required IEnumerable<SpotifyArtist> Artists { get; set; }
}
