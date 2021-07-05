using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stock.Analysis._0607.Service;

namespace Stock.Analysis._0607
{
    class Program
    {
        private static IMovingAvarageService _movingAvgService = new MovingAvarageService();
        private static IFileHandler _fileHandler = new FileHandler();
        private static IDataService _dataService = new DataService();
        static void Main(string[] args)
        {
            var stockSymbol = "2603.TW";
            var path = Path.Combine(Environment.CurrentDirectory, @"Data/20100101-20210531.csv");
            List<ChartData> chartDataList = GetMaFromYahoo(stockSymbol);
            _fileHandler.OutputResult(chartDataList);
        }

        private static List<ChartData> GetMaFromYahoo(string stockSymbol)
        {
            var chartDataList = new List<ChartData>();
            var stockList = _dataService.Get1YDataFromYahooApi(stockSymbol);

            Console.WriteLine($"{stockSymbol}");
            var chartData = new ChartData();
            chartData.Name = $"{stockSymbol}";
            chartData.Min = stockList.Where(stock => stock.Price != null).Min(stock=> (double)stock.Price);
            chartData.Max = stockList.Where(stock => stock.Price != null).Max(stock=> (double)stock.Price);
            chartData.Day = stockList.Select(s => s.Date.ToString()).ToList();
            chartData.Price = stockList.Select(s => s.Price).ToList();
            chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5);
            chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10);
            chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20);
            chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60);

            chartDataList.Add(chartData);
            return chartDataList;
        }

        private static List<ChartData> GetMaFromCsv(string path)
        {
            var stockData = _fileHandler.ReadDataFromFile(path);
            var index = 0;
            var chartDataList = new List<ChartData>();
            stockData.ForEach(stockList =>
            {
                Console.WriteLine($"Stock {index}");
                var chartData = new ChartData();
                chartData.Name = $"Stock {index}";
                chartData.Day = stockList.Select(s => s.Date.ToString()).ToList();
                chartData.Price = stockList.Select(s => s.Price).ToList();
                chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5);
                chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10);
                chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20);
                chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60);

                chartDataList.Add(chartData);
                index++;
            });
            return chartDataList;
        }
    }
}
