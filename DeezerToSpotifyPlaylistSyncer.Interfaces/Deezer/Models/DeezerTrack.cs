namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;

public class DeezerTrack
{
	public required long Id { get; set; }
	public required string Title { get; set; }
	public required DeezerArtist Artist { get; set; }
	public IEnumerable<DeezerArtist>? Contributors { get; set; }
}