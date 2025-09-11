namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Configuration;

public class SpotifyConfiguration
{
	public required string ClientId { get; set; }
	public required string ClientSecret { get; set; } 
	public required string RefreshToken { get; set; }
	public required string BaseUrl { get; set; }
	public required string PlaylistId { get; set; }
}
