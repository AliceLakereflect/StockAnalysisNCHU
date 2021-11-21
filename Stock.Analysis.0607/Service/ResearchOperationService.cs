using System;
using System.Collections.Generic;
using System.Linq;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Service
{
    public class ResearchOperationService: IResearchOperationService
    {
        private IMovingAvarageService _movingAvgService = new MovingAvarageService();
        private ITransTimingService _transTimingService = new TransTimingService();
        private ICalculateVolumeService _calculateVolumeService = new CalculateVolumeService();
        private IFileHandler _fileHandler = new FileHandler();
        public ResearchOperationService()
        {
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public ChartData GetMaFromYahoo(string symbol, List<StockModel> stockList)
        {
            var chartData = new ChartData();
            chartData.Name = $"{symbol}";
            chartData.Min = stockList.Where(stock => stock.Price != null).Min(stock => (double)stock.Price);
            chartData.Max = stockList.Where(stock => stock.Price != null).Max(stock => (double)stock.Price);
            chartData.Day = stockList.Select(s => {
                var currentDateTime = UnixTimeStampToDateTime(s.Date);
                return $"{currentDateTime.Year}/{currentDateTime.Month}/{currentDateTime.Day}";
                }).ToList();
            chartData.Timestamp = stockList.Select(s => s.Date).ToList();
            chartData.Price = stockList.Select(s => s.Price).ToList();
            chartData.PriceAvg5Days = _movingAvgService.CalculateMovingAvarage(stockList, 5).Select(s => s.Price).ToList();
            chartData.PriceAvg10Days = _movingAvgService.CalculateMovingAvarage(stockList, 10).Select(s => s.Price).ToList();
            chartData.PriceAvg20Days = _movingAvgService.CalculateMovingAvarage(stockList, 20).Select(s => s.Price).ToList();
            chartData.PriceAvg60Days = _movingAvgService.CalculateMovingAvarage(stockList, 60).Select(s => s.Price).ToList();

            return chartData;
        }

        public List<ChartData> GetMaFromCsv(string path)
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

        public string Settlement(double currentStock,List<StockTransaction> myTrans, TestCase ma, double periodEnd)
        {
            var hasQty = myTrans.Last().TransType == TransactionType.Buy;
            if (hasQty)
            {
                var timeString = Utils.UnixTimeStampToDateTime(periodEnd);
                var price = Math.Round(currentStock, 2, MidpointRounding.AwayFromZero);
                myTrans.Add(new StockTransaction
                {
                    TransTime = periodEnd,
                    TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                    TransPrice = price,
                    TransType = TransactionType.Sell,
                    TransVolume = myTrans.Last().TransVolume,
                    Balance = myTrans.Last().Balance + Math.Round(currentStock * myTrans.Last().TransVolume)
                });
            }
            var buy = myTrans.Where(trans => trans.TransType == TransactionType.Buy)
                .Sum(trans => Math.Round(trans.TransPrice * trans.TransVolume));
            var sell = myTrans.Where(trans => trans.TransType == TransactionType.Sell)
                .Sum(trans => Math.Round(trans.TransPrice * trans.TransVolume));
            var earn = sell - buy;
            var resultString = $"When ma = {ma.ShortTermMa} vs {ma.LongTermMa},\tEarned: {earn}\t";
            return resultString;
        }

        public List<StockTransaction> GetMyTransactions(double funds, ChartData data, List<StockModel> stockList, int shortTermMa, int LongTermMa)
        {
            var myTransactions = new List<StockTransaction>();
            myTransactions.Add(new StockTransaction
            {
                TransTime = 0,
                TransTimeString = string.Empty,
                TransPrice = 0,
                TransVolume = 0,
                TransType = TransactionType.AddFunds,
                Balance = funds
            });
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
            var check = false;
            double maxPrice = 0;
            data.Timestamp.ForEach(timestamp =>
            {
                var shortMaVal = shortMaValList[index];
                var longMaVal = longMaValList[index];
                var timeString = Utils.UnixTimeStampToDateTime(timestamp);
                if (shortMaVal != null && longMaVal != null)
                {
                    var price = data.Price[index] ?? 0;
                    price = Math.Round(price, 2, MidpointRounding.AwayFromZero);
                    var lastTrans = myTransactions.Any() ? myTransactions.LastOrDefault() : new StockTransaction();
                    var lastTransTime = lastTrans.TransTime;
                    if (_transTimingService.TimeToBuy(shortMaVal, longMaVal, hasQty, check))
                    {
                        var volume = _calculateVolumeService.CalculateBuyingVolume(lastTrans.Balance, price);
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = timestamp,
                            TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                            TransPrice = price,
                            TransType = TransactionType.Buy,
                            TransVolume = volume,
                            Balance = lastTrans.Balance - Math.Round(price * volume)
                        });
                        hasQty = !hasQty;
                    }
                    else if (_transTimingService.TimeToSell(shortMaVal, longMaVal, hasQty))
                    {
                        var volume = _calculateVolumeService.CalculateSellingVolume(myTransactions.LastOrDefault().TransVolume);
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = timestamp,
                            TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                            TransPrice = price,
                            TransType = TransactionType.Sell,
                            TransVolume = volume,
                            Balance = lastTrans.Balance + Math.Round(price * volume)
                        });
                        hasQty = !hasQty;
                        maxPrice = 0;
                    }
                    check = _transTimingService.TrueCheckGoldCross(check, shortMaVal, longMaVal);
                }

                index++;
            });

            return myTransactions;
        }
        public List<StockTransaction> GetMyTransactionsOddShares(double funds, ChartData data, List<StockModel> stockList, int shortTermMa, int LongTermMa)
        {
            var myTransactions = new List<StockTransaction>();
            myTransactions.Add(new StockTransaction
            {
                TransTime = 0,
                TransTimeString = string.Empty,
                TransPrice = 0,
                TransVolume = 0,
                TransType = TransactionType.AddFunds,
                Balance = funds
            });
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
            var check = false;
            double maxPrice = 0;
            data.Timestamp.ForEach(timestamp =>
            {
                var shortMaVal = shortMaValList[index];
                var longMaVal = longMaValList[index];
                var timeString = Utils.UnixTimeStampToDateTime(timestamp);
                if (shortMaVal != null && longMaVal != null)
                {
                    var price = data.Price[index] ?? 0;
                    price = Math.Round(price, 2, MidpointRounding.AwayFromZero);
                    var lastTrans = myTransactions.Any() ? myTransactions.LastOrDefault() : new StockTransaction();
                    if (_transTimingService.TimeToBuy(shortMaVal, longMaVal, hasQty, check))
                    {
                        var volume = _calculateVolumeService.CalculateBuyingVolumeOddShares(lastTrans.Balance, price);
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = timestamp,
                            TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                            TransPrice = price,
                            TransType = TransactionType.Buy,
                            TransVolume = volume,
                            Balance = lastTrans.Balance - Math.Round(price * volume, MidpointRounding.ToZero)
                        });
                        hasQty = !hasQty;
                    }
                    // todo: 停損比例改為參數，從testcase丟進來
                    else if (_transTimingService.TimeToSell(lastTrans, ref maxPrice, price,timestamp, 10, hasQty))
                    {
                        var volume = _calculateVolumeService.CalculateSellingVolume(myTransactions.LastOrDefault().TransVolume);
                        myTransactions.Add(new StockTransaction
                        {
                            TransTime = timestamp,
                            TransTimeString = $"{timeString.Year}-{timeString.Month}-{timeString.Day}",
                            TransPrice = price,
                            TransType = TransactionType.Sell,
                            TransVolume = volume,
                            Balance = lastTrans.Balance + Math.Round(price * volume, MidpointRounding.ToZero)
                        });
                        hasQty = !hasQty;
                        maxPrice = 0;
                    }
                    check = _transTimingService.TrueCheckGoldCross(check, shortMaVal, longMaVal);
                }

                index++;
            });

            return myTransactions;
        }
    }

    public interface IResearchOperationService
    {
        List<ChartData> GetMaFromCsv(string path);
        ChartData GetMaFromYahoo(string symbol, List<StockModel> stockList);
        string Settlement(double currentStock, List<StockTransaction> myTrans, TestCase ma, double periodEnd);
        List<StockTransaction> GetMyTransactions(double funds, ChartData data, List<StockModel> stockList, int shortTermMa, int LongTermMa);
        List<StockTransaction> GetMyTransactionsOddShares(double funds, ChartData data, List<StockModel> stockList, int shortTermMa, int LongTermMa);
    }
}
