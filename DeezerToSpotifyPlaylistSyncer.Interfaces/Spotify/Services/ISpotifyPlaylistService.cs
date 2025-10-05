using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Models;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Models;

namespace DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Services;

public interface ISpotifyPlaylistService
{
	Task<SpotifyPlaylist?> GetPlaylistAsync();
	Task<IList<SpotifyTrack>> GetTrackIdsAsync(IEnumerable<DeezerTrack> detailedDeezerTracks);
	Task<IList<SpotifyTrack>?> AddMissingTracksAsync(SpotifyPlaylist spotifyPlaylist, IList<SpotifyTrack> spotifyTracks);
	Task<IList<SpotifyTrack>?> RemoveOldTracksAsync(SpotifyPlaylist spotifyPlaylist, IList<SpotifyTrack> spotifyTracks);
}
