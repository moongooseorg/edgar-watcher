using EdgarWatcher.Configuration;
using EdgarWatcher.Features;
using EdgarWatcher.Features.SecApi;
using EdgarWatcher.Features.Webhook;
using OpenBaoConfiguration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddOpenBaoConfiguration();

builder.Services.AddHttpClient<SecApiService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
    });

builder.Services.AddHttpClient<DiscordSecMessenger>();

builder.Services.AddHostedService<WatcherService>();

WebApplication app = builder.Build();

app.MapGet("/healthcheck", () => Results.Json(new { status = "up" }));

app.Run();
