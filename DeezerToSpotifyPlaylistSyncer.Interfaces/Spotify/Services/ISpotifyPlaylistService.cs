using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Services;

public interface ISpotifyPlaylistService
{
	Task<SpotifyPlaylist?> GetPlaylistAsync();
	Task<IList<string>> GetTrackIdsAsync(DeezerPlaylist deezerPlaylist);
	Task AddMissingTracksAsync(SpotifyPlaylist spotifyPlaylist, IList<string> spotifyTrackIds);
}
