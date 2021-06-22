using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stock.Analysis._0607
{
    class Program
    {
        private static IMovingAvarageService _movingAvgService = new MovingAvarageService();
        private static IFileHandler _fileHandler = new FileHandler();
        static void Main(string[] args)
        {
            var path = Path.Combine(Environment.CurrentDirectory, @"Data/20100101-20210531.csv");
            var stockData = _fileHandler.ReadData(path);
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
            _fileHandler.OutputResult(chartDataList);
        }

    }
}
