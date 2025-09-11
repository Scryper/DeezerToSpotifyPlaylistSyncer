namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;

public class DeezerTrack
{
	public required string Title { get; set; }
	public required DeezerArtist Artist { get; set; }
}