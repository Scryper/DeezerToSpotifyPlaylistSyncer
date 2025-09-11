using System.Net.Http.Headers;
using DeezerToSpotifyPlaylistSyncer.Function.Authentication;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Configuration;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Services;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Configuration;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Spotify.Services;
using DeezerToSpotifyPlaylistSyncer.Services.Deezer;
using DeezerToSpotifyPlaylistSyncer.Services.Spotify;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
	.ConfigureFunctionsApplicationInsights();

builder.Services.AddOptions<DeezerConfiguration>().Configure<IConfiguration>((settings, configuration) => configuration.GetSection(nameof(DeezerConfiguration)).Bind(settings));
var deezerConfiguration = builder.Configuration.GetSection(nameof(DeezerConfiguration));
builder.Services.AddHttpClient<IDeezerPlaylistService, DeezerPlaylistService>(client =>
	client.BaseAddress = new Uri(deezerConfiguration[nameof(DeezerConfiguration.BaseUrl)] ?? throw new InvalidOperationException("Configuration is null")));

builder.Services.AddOptions<SpotifyConfiguration>().Configure<IConfiguration>((settings, configuration) => configuration.GetSection(nameof(SpotifyConfiguration)).Bind(settings));
var spotifyConfiguration = builder.Configuration.GetSection(nameof(SpotifyConfiguration));
builder.Services.AddHttpClient<ISpotifyPlaylistService, SpotifyPlaylistService>(client =>
{
	client.BaseAddress = new Uri(spotifyConfiguration[nameof(SpotifyConfiguration.BaseUrl)] ?? throw new InvalidOperationException("Configuration is null"));
	client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SpotifyTokenProvider.GetAccessTokenAsync(
		spotifyConfiguration[nameof(SpotifyConfiguration.ClientId)] ?? throw new InvalidOperationException("Configuration is null"),
		spotifyConfiguration[nameof(SpotifyConfiguration.ClientSecret)] ?? throw new InvalidOperationException("Configuration is null"),
		spotifyConfiguration[nameof(SpotifyConfiguration.RefreshToken)] ?? throw new InvalidOperationException("Configuration is null")).Result);
});

await builder.Build().RunAsync();