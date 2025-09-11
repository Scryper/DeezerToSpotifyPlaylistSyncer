namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;

public class SpotifyTrack
{
	public string Id { get; set; } = string.Empty;
	public SpotifyTrackItem? Track { get; set; }
	public string? Name { get; set; }
	public IEnumerable<SpotifyArtist>? Artists { get; set; }
}
