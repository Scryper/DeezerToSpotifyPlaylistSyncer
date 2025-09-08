using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Configuration;
using DeezerToSpotifyPlaylistSyncer.Interfaces.Deezer.Services;
using DeezerToSpotifyPlaylistSyncer.Services.Deezer;
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

await builder.Build().RunAsync();