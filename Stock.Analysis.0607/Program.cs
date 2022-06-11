using Stock.Analysis._0607.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Stock.Analysis._0607.Repository;
using Microsoft.EntityFrameworkCore;
using Stock.Analysis._0607.Interface;
using Stock.Analysis._0607.Models;
using Microsoft.Extensions.Logging;

namespace Stock.Analysis._0607
{
    class Program
    {

        public static readonly ILoggerFactory MyLoggerFactory =
           LoggerFactory.Create(
                builder =>
                {
                    builder.AddConsole().AddFilter(level => level == LogLevel.Critical);
                }
           );

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            
        }
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
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
            services.AddSingleton<IOutputResultService, OutputResultService>();

            // DbContext
            var connectString = "Host=localhost;Database=StockResearch;Username=postgres;Password=13";
            services.AddDbContextPool<StockModelDbContext>(options => {
                options.UseNpgsql(connectString).UseLoggerFactory(MyLoggerFactory);
                });
            services.AddScoped<IDataProvider<StockModel>, StockModelDataProvider>();
            services.AddDbContextPool<TrainResultDbContext>(options => options.UseNpgsql(connectString).UseLoggerFactory(MyLoggerFactory));
            services.AddScoped<IDataProvider<TrainResult>, TrainResultProvider>();
            services.AddDbContextPool<TrainBestTransactionDbContext>(options => options.UseNpgsql(connectString).UseLoggerFactory(MyLoggerFactory));
            services.AddScoped<IDataProvider<TrainBestTransaction>, TrainBestTransactionProvider>();

            // automapper
            services.AddAutoMapper(typeof(StockModelDTO));

            services.AddLogging();

        }
    }
}
