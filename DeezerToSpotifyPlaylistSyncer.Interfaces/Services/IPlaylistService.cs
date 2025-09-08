using DeezerToSpotifyPlaylistSyncer.Interfaces.Models;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Services;

public interface IPlaylistService
{
	public Task<IPlaylist?> GetPlaylistAsync(string playlistId);
}
