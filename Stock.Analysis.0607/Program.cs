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
            var stockSymbol = new List<string> { "2603.TW", "2609.TW", "2615.TW" };
            var chartDataList = new List<ChartData>();
            var myTransList = new List<StockTransList>();
            List<TestCase> testCase = new List<TestCase> {
                new TestCase { ShortTermMa = 5, LongTermMa = 20 },
                new TestCase { ShortTermMa = 5, LongTermMa = 60 },
                new TestCase { ShortTermMa = 20, LongTermMa = 60 },
                new TestCase { ShortTermMa = 6, LongTermMa = 18 },
                new TestCase { ShortTermMa = 6, LongTermMa = 50 },
                new TestCase { ShortTermMa = 18, LongTermMa = 50 },
            };
            stockSymbol.ForEach(symbol =>
            {
                var chartDataList = new List<ChartData>();
                var stockList = _dataService.GetPeriodDataFromYahooApi(symbol, new DateTime(2020,1,1,0,0,0), new DateTime(2021, 7, 1, 0, 0, 0));
                ChartData data = GetMaFromYahoo(symbol, stockList);
                Console.WriteLine($"{symbol}\t Current Stock:{stockList.Last().Price}");
                chartDataList.Add(data);
                testCase.ForEach(ma =>
                {
                    var myTrans = GetMyTransactions(data, stockList, ma.ShortTermMa, ma.LongTermMa);
                    myTransList.Add(new StockTransList { Name = symbol, TestCase = ma, Transactions = myTrans });
                    string resultString = Settlement(data, myTrans, ma);
                    var transString = "=== My Transaction ======================================================================================\n";
                    transString += $"|\tName: {symbol}\t Test Case: {ma.ShortTermMa}MA vs {ma.LongTermMa}MA \t Current Stock:{stockList.Last().Price}\n";
                    myTrans.ForEach(t=>
                    {
                        transString += $"|\tTransTime: {t.TransTime}\t TransVolume:{t.TransVolume}\t Fees:{t.Fees}\t Tax: {t.Tax}\t TransPrice: {t.TransPrice}\n";
                    });
                    transString +=    "=========================================================================================================\n";
                    //Console.WriteLine(transString);
                });
            });

            _fileHandler.OutputResult(chartDataList, "chartData");
            _fileHandler.OutputResult(myTransList, "myTransaction");
            
        }

        private static string Settlement(ChartData data, List<StockTransaction> myTrans, TestCase ma)
        {
            var hasQty = myTrans.Last().TransVolume == 1;
            var buy = myTrans.Where(trans => trans.TransVolume == 1).Sum(trans => trans.TransPrice);
            var sell = myTrans.Where(trans => trans.TransVolume == -1).Sum(trans => trans.TransPrice);
            var earn = hasQty ? sell - buy + myTrans.Last().TransPrice * myTrans.Last().TransVolume : sell - buy;
            var resultString = hasQty
                                ? $"When ma = {ma.ShortTermMa} vs {ma.LongTermMa},\tEarned: {earn}\t(Include holdings: {myTrans.Last().TransPrice})"
                                : $"When ma = {ma.ShortTermMa} vs {ma.LongTermMa},\tEarned: {earn}\t";
            Console.WriteLine(resultString);
            return resultString;
        }

        private static List<StockTransaction> GetMyTransactions(ChartData data, List<StockModel> stockList, int shortTermMa, int LongTermMa)
        {
            var myTransactions = new List<StockTransaction>();
            var symbol = data.Name;
            var index = 0;
            List<double?> shortMaValList;
            List<double?> longMaValList;
            shortMaValList = !data.GetMaValue(shortTermMa).Any()
                ? _movingAvgService.CalculateMovingAvarage(stockList, shortTermMa).Select(stock => stock.Price).ToList()
                : data.GetMaValue(shortTermMa);

            longMaValList = !data.GetMaValue(LongTermMa).Any()
                ? _movingAvgService.CalculateMovingAvarage(stockList, LongTermMa).Select(stock => stock.Price).ToList()
                : data.GetMaValue(LongTermMa);

            bool hasQty = false;
            data.Timestamp.ForEach(timestamp =>
            {
                var shortMaVal = shortMaValList[index];
                var LongMaVal = longMaValList[index];
                if (shortMaVal != null && LongMaVal != null)
                {
                    if (shortMaVal > LongMaVal && hasQty == false)
                    {
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = data.Timestamp[index],
                            TransPrice = data.Price[index] ?? 0,
                            TransVolume = 1
                        });
                        hasQty = !hasQty;
                    }
                    else if (shortMaVal < LongMaVal && hasQty == true)
                    {
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = data.Timestamp[index],
                            TransPrice = data.Price[index] ?? 0,
                            TransVolume = -1
                        });
                        hasQty = !hasQty;
                    }
                }
                
                index++;
            });

            return myTransactions;
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private static ChartData GetMaFromYahoo(string symbol, List<StockModel> stockList)
        {
            var chartData = new ChartData();
            chartData.Name = $"{symbol}";
            chartData.Min = stockList.Where(stock => stock.Price != null).Min(stock => (double)stock.Price);
            chartData.Max = stockList.Where(stock => stock.Price != null).Max(stock => (double)stock.Price);
            chartData.Day = stockList.Select(s => UnixTimeStampToDateTime(s.Date).ToString()).ToList();
            chartData.Timestamp = stockList.Select(s => s.Date).ToList();
            chartData.Price = stockList.Select(s => s.Price).ToList();
            chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5).Select(s => s.Price).ToList();
            chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10).Select(s => s.Price).ToList();
            chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20).Select(s => s.Price).ToList();
            chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60).Select(s => s.Price).ToList();

            return chartData;
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
                chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5).Select(s => s.Price).ToList();
                chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10).Select(s => s.Price).ToList();
                chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20).Select(s => s.Price).ToList();
                chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60).Select(s => s.Price).ToList();

                chartDataList.Add(chartData);
                index++;
            });
            return chartDataList;
        }
    }
}
