using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Newtonsoft.Json;

namespace Stock.Analysis._0607
{
    class Program
    {
        private static IMovingAvarageService _movingAvgService = new MovingAvarageService();
        static void Main(string[] args)
        {
            var stockData = ReadData();
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
            OutputResult(chartDataList);
        }

        private static void OutputResult(List<ChartData> chartDataList)
        {
            var path = Path.Combine(Environment.CurrentDirectory, @"Output/chartData.json");
            var jsonOutput = JsonConvert.SerializeObject(chartDataList);

            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            File.WriteAllText(file.FullName, jsonOutput);
        }

        

        private static List<List<StockModel>> ReadData()
        {
            var path = Path.Combine(Environment.CurrentDirectory, @"Data/20100101-20210531.csv");
            Console.WriteLine($"Getting data from {path}");
            var stockList1 = new List<StockModel>();
            var stockList2 = new List<StockModel>();
            var stockList3 = new List<StockModel>();
            var stockList4 = new List<StockModel>();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                while (csv.Read())
                {
                    if (string.IsNullOrEmpty(csv.GetField(0)) || csv.GetField(0) == "???")
                    {
                        continue;
                    }
                    DateTime.TryParse(csv.GetField(0), out var datetime);
                    double.TryParse(csv.GetField(1), out var stock1);
                    stockList1.Add(new StockModel
                    {
                        Date = datetime,
                        Price = stock1
                    });

                    double.TryParse(csv.GetField(2), out var stock2);
                    stockList2.Add(new StockModel
                    {
                        Date = datetime,
                        Price = stock2
                    });

                    double.TryParse(csv.GetField(3), out var stock3);
                    stockList3.Add(new StockModel
                    {
                        Date = datetime,
                        Price = stock3
                    });

                    double.TryParse(csv.GetField(4), out var stock4);
                    stockList4.Add(new StockModel
                    {
                        Date = datetime,
                        Price = stock4
                    });
                }
            }
            var result = new List<List<StockModel>>();
            result.Add(stockList1);
            result.Add(stockList2);
            result.Add(stockList3);
            result.Add(stockList4);

            return result;
        }
    }
}
