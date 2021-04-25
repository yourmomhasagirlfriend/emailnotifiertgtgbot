﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class TooGoodToGoNotifierWorker : BackgroundService
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IServiceProvider _serviceProvider;
        private readonly SchedulerOptions _schedulerOptions;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public TooGoodToGoNotifierWorker(IHostEnvironment hostEnvironment, IHostApplicationLifetime hostApplicationLifetime, IServiceProvider serviceProvider, IOptions<SchedulerOptions> schedulerOptions)
        {
            _hostEnvironment = hostEnvironment;
            _serviceProvider = serviceProvider;
            _schedulerOptions = schedulerOptions.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serviceProvider.UseScheduler(scheduler =>
            {
                var scheduleInterval = scheduler.Schedule<FavoriteBasketsWatcher>();
                var scheduledEventConfiguration = _hostEnvironment.IsDevelopment() ? scheduleInterval.EveryFiveSeconds() : scheduleInterval.Cron(_schedulerOptions.CronExpression);
                scheduledEventConfiguration
                .Zoned(TimeZoneInfo.Local)
                .PreventOverlapping(nameof(FavoriteBasketsWatcher));
            })
            .LogScheduledTaskProgress(_serviceProvider.GetService<ILogger<IScheduler>>())
            .OnError((exception) =>
            {
                _hostApplicationLifetime.StopApplication();
            });

            return Task.CompletedTask;
        }
    }
}
