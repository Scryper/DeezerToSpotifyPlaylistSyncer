using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Services;

public interface IDeezerPlaylistService
{
	Task<DeezerPlaylist?> GetPlaylistAsync();
	Task<IEnumerable<DeezerTrack>> GetDetailedTracksAsync(IEnumerable<long> ids);
}
