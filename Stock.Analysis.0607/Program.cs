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
            var stockSymbol = new List<string>{ "2603.TW", "2609.TW", "2615.TW" };
            var myTransactions = new List<StockTransaction>();

            const int shortTermMa = 5;
            const int LongTermMa = 20;
            List<ChartData> chartDataList = GetMaFromYahoo(stockSymbol);
            chartDataList.ForEach(data=>
            {

            });


            _fileHandler.OutputResult(chartDataList);
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private static List<ChartData> GetMaFromYahoo(List<string> stockSymbolList)
        {
            var chartDataList = new List<ChartData>();
            stockSymbolList.ForEach(symbol => {
                var stockList = _dataService.Get1YDataFromYahooApi(symbol);

                Console.WriteLine($"{symbol}");
                var chartData = new ChartData();
                chartData.Name = $"{symbol}";
                chartData.Min = stockList.Where(stock => stock.Price != null).Min(stock => (double)stock.Price);
                chartData.Max = stockList.Where(stock => stock.Price != null).Max(stock => (double)stock.Price);
                chartData.Day = stockList.Select(s => UnixTimeStampToDateTime(s.Date).ToString()).ToList();
                chartData.Price = stockList.Select(s => s.Price).ToList();
                chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5).OrderBy(s => s.Date).Select(s => s.Price).ToList();
                chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10).OrderBy(s => s.Date).Select(s => s.Price).ToList();
                chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20).OrderBy(s => s.Date).Select(s => s.Price).ToList();
                chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60).OrderBy(s => s.Date).Select(s => s.Price).ToList();
                chartDataList.Add(chartData);
            });
            

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
                chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5).OrderBy(s => s.Date).Select(s => s.Price).ToList();
                chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10).OrderBy(s => s.Date).Select(s => s.Price).ToList();
                chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20).OrderBy(s => s.Date).Select(s => s.Price).ToList();
                chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60).OrderBy(s => s.Date).Select(s => s.Price).ToList();

                chartDataList.Add(chartData);
                index++;
            });
            return chartDataList;
        }
    }
}
