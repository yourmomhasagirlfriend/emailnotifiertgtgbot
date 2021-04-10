﻿using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestSharp;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            var favoriteBasketsWatcher = host.Services.GetRequiredService<FavoriteBasketsWatcher>();
            favoriteBasketsWatcher.Start();

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConsole();
            })
            .ConfigureServices((host, services) =>
            {
                services.AddLogging()
                .Configure<ApiOptions>(host.Configuration.GetSection(nameof(ApiOptions)))
                .Configure<WatcherOptions>(host.Configuration.GetSection(nameof(WatcherOptions)))
                .AddSingleton<IRestClient, RestClient>(serviceProvider => new RestClient { ThrowOnAnyError = true })
                .AddSingleton<ITooGoodToGoApiService, TooGoodToGoApiService>()
                .AddSingleton<IEmailNotifier, EmailNotifier>()
                .AddSingleton<FavoriteBasketsWatcher>();
            });
    }
}