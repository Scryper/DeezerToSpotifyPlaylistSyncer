namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;

public class SpotifyTracks
{
	public List<SpotifyTrack> Items { get; set; }
	public string? Next { get; set; }
}
