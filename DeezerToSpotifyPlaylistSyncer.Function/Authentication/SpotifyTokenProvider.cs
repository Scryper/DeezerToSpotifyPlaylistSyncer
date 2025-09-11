using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Configuration;

namespace DeezerToSpotifyPlaylistSyncer.Function.Authentication;

public static class SpotifyTokenProvider
{
	public static async Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string refreshToken)
	{
		var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

		var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
		request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
		request.Content = new FormUrlEncodedContent([
			new KeyValuePair<string?, string?>("grant_type", "refresh_token"),
			new KeyValuePair<string?, string?>("refresh_token", refreshToken)
		]);

		using var http = new HttpClient();
		var response = await http.SendAsync(request);
		response.EnsureSuccessStatusCode();

		var credentials = JsonSerializer.Deserialize<SpotifyCredentials>(await response.Content.ReadAsStringAsync());
		return credentials?.AccessToken ?? throw new Exception("Invalid client credentials");
	}
}
