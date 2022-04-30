using Stock.Analysis._0607.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Stock.Analysis._0607
{
    class Program
    {
       
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            
        }
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) => {
                services.AddHostedService<Worker>();
                ConfigureServices(hostContext.Configuration, services);
            });


        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IResearchOperationService, ResearchOperationService>();
            services.AddSingleton<IGNQTSAlgorithmService, GNQTSAlgorithmService>();
            services.AddSingleton<IQTSAlgorithmService, QTSAlgorithmService>();
            services.AddSingleton<IFileHandler, FileHandler>();
            services.AddSingleton<IDataService, DataService>();
            services.AddSingleton<ISlidingWindowService, SlidingWindowService>();
            services.AddSingleton<IMovingAvarageService, MovingAvarageService>();
            services.AddSingleton<ITransTimingService, TransTimingService>();
            services.AddSingleton<ICalculateVolumeService, CalculateVolumeService>();
        }

    }
}
