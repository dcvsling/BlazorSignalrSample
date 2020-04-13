using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorApp1.Data;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            
            services.AddSignalR();
            
            services.AddHostedService<MyBackgroundService>();
            services.AddSingleton<WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TickerHub>("ticker");
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }

    public class TickerHub : Hub<IClientProxy>
    {
        public Task SendTickerAsync(string msg)
            => Clients.All.SendAsync("ticker", msg);
    }

    public class MyBackgroundService : BackgroundService
    {
        private readonly IHubContext<TickerHub> _context;
        private readonly ILogger<MyBackgroundService> _logger;

        public MyBackgroundService(IHubContext<TickerHub> context, ILogger<MyBackgroundService> logger) : base()
        {
            _context = context;
            _logger = logger;
        }

        async protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            while(!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(2000);
                var ticker = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                await _context.Clients.All.SendAsync(nameof(ticker), ticker);
                _logger.LogInformation(ticker);
            }
        }
    }
}
