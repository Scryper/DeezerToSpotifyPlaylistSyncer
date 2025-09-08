using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Services;

public interface IDeezerPlaylistService
{
	public Task<Playlist?> GetPlaylistAsync();
}
